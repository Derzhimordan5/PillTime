using PillTime.Models;
using Plugin.LocalNotification;

namespace PillTime.Views
{
    public partial class AddMedicinePage : ContentPage
    {
        // Список единиц для врача
        readonly List<string> _units = new()
        {
            "ампула", "грамм", "капля", "капсула", "пакетик", "таблетка"
        };

        public AddMedicinePage()
        {
            InitializeComponent();

            // Наполнение единиц
            DoctorUnitPicker.ItemsSource = _units;
        }

        // Переключение блоков в зависимости от выбора режима
        private void OnModeChanged(object sender, EventArgs e)
        {
            bool isDoctor = ModePicker.SelectedIndex == 0;
            DoctorBlock.IsVisible = isDoctor;
            SelfBlock.IsVisible = !isDoctor;
        }

        // При вводе числа приёмов для врача динамическое создание строк
        private void OnIntakesPerDayChanged(object sender, EventArgs e)
        {
            DoctorScheduleStack.Children.Clear();

            if (!int.TryParse(IntakesPerDayEntry.Text, out int count) || count <= 0)
                return;

            for (int i = 1; i <= count; i++)
            {
                var timePicker = new TimePicker
                {
                    Time = new TimeSpan(8, 0, 0),
                    Format = "HH:mm"
                };
                var doseEntry = new Entry
                {
                    Keyboard = Keyboard.Numeric,
                    Placeholder = $"{i}-я доза ({DoctorUnitPicker.SelectedItem})"
                };
                var row = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Label { Text = $"Приём {i}:", VerticalTextAlignment = TextAlignment.Center },
                        timePicker,
                        doseEntry
                    }
                };
                DoctorScheduleStack.Children.Add(row);
            }
        }

        // Создание строк при вводе числа приёмов для самостоятельного курса 
        private void OnIntakesPerDaySelfChanged(object sender, EventArgs e)
        {
            SelfScheduleStack.Children.Clear();

            if (!int.TryParse(IntakesPerDaySelfEntry.Text, out int count) || count <= 0)
                return;

            double.TryParse(MaxDailyDosePerKgEntry.Text?.Replace(',', '.'),
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out double maxPerKg);

            double weight = Preferences.Get("Weight", 0.0);
            double maxDailyDose = weight * maxPerKg;
            double dosePerIntake = (count > 0) ? maxDailyDose / count : 0;

            for (int i = 1; i <= count; i++)
            {
                var timePicker = new TimePicker
                {
                    Time = new TimeSpan(8, 0, 0),
                    Format = "HH:mm"
                };
                var doseLabel = new Label
                {
                    Text = dosePerIntake > 0 ? $"{dosePerIntake:F2} мг" : $"{i}-я доза: —",
                    VerticalTextAlignment = TextAlignment.Center
                };
                var row = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children =
            {
                new Label { Text = $"Приём {i}:", VerticalTextAlignment = TextAlignment.Center },
                timePicker,
                doseLabel
            }
                };
                SelfScheduleStack.Children.Add(row);
            }
        }


        private async void OnSaveClicked(object sender, EventArgs e)
        {
            string name = NameEntry.Text?.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Ошибка", "Введите название лекарства.", "ОК");
                return;
            }
            var specialInstructions = SpecialInstructionsEntry.Text?.Trim();

            // Режим "По назначению врача"
            if (ModePicker.SelectedIndex == 0)
            {
                var unit = DoctorUnitPicker.SelectedItem as string;
                if (unit == null)
                {
                    await DisplayAlert("Ошибка", "Выберите единицу измерения.", "ОК");
                    return;
                }

                if (!int.TryParse(IntakesPerDayEntry.Text, out int intakesPerDay) || intakesPerDay <= 0)
                {
                    await DisplayAlert("Ошибка", "Введите корректное число приёмов в день.", "ОК");
                    return;
                }

                var times = new List<string>();
                var doses = new List<string>();
                double dailyUsage = 0;

                foreach (var child in DoctorScheduleStack.Children)
                {
                    if (child is HorizontalStackLayout row &&
                        row.Children[1] is TimePicker tp &&
                        row.Children[2] is Entry de &&
                        double.TryParse(de.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double doseVal))
                    {
                        times.Add(tp.Time.ToString(@"hh\:mm"));
                        doses.Add($"{doseVal} {unit}");
                        dailyUsage += doseVal;
                    }
                }

                if (times.Count != intakesPerDay)
                {
                    await DisplayAlert("Ошибка", "Не все приёмы заполнены.", "ОК");
                    return;
                }

                if (!double.TryParse(StockEntry.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double stock) || stock <= 0)
                {
                    await DisplayAlert("Ошибка", "Введите корректный запас лекарства.", "ОК");
                    return;
                }

                int daysAvailable = dailyUsage > 0 ? (int)Math.Floor(stock / dailyUsage) : 0;

                var med = new Medicine
                {
                    Name = name,
                    Unit = unit,
                    IntakesPerDay = intakesPerDay,
                    IntakeTimes = times.ToArray(),
                    IntakeDoses = doses.ToArray(),
                    StockAmount = stock,
                    PackageCount = (int)Math.Floor(stock),
                    DaysAvailable = daysAvailable,
                    SpecialInstructions = specialInstructions
                };

                await App.Database.SaveMedicineAsync(med);
                ScheduleMedicineNotifications(med.Name, times);
                await Navigation.PopAsync();

            }

            // Режим "Самостоятельный курс"
            if (ModePicker.SelectedIndex == 1)
            {
                var rawMaxPerKg = MaxDailyDosePerKgEntry.Text?.Replace(',', '.') ?? "";
                var rawIntakesPerDay = IntakesPerDaySelfEntry.Text ?? "";
                var rawStockSelf = StockSelfEntry.Text?.Replace(',', '.') ?? "";

                if (!double.TryParse(rawMaxPerKg, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double maxPerKg) || maxPerKg <= 0 ||
                    !int.TryParse(rawIntakesPerDay, out int intakesPerDaySelf) || intakesPerDaySelf <= 0)
                {
                    await DisplayAlert("Ошибка", "Введите корректные данные.", "ОК");
                    return;
                }

                double weight = Preferences.Get("Weight", 0.0);
                if (weight <= 0)
                {
                    await DisplayAlert("Ошибка", "Не указан вес пациента в профиле.", "ОК");
                    return;
                }

                double maxDailyDose = weight * maxPerKg;
                double dosePerIntake = maxDailyDose / intakesPerDaySelf;

                var times = new List<string>();
                var doses = new List<string>();

                foreach (var child in SelfScheduleStack.Children)
                {
                    if (child is HorizontalStackLayout row && row.Children[1] is TimePicker tp)
                    {
                        times.Add(tp.Time.ToString(@"hh\:mm"));
                        doses.Add($"{dosePerIntake:F2} мг");

                        if (row.Children[2] is Label lbl)
                            lbl.Text = $"{dosePerIntake:F2} мг";
                    }
                }

                if (times.Count != intakesPerDaySelf)
                {
                    await DisplayAlert("Ошибка", "Не все приёмы заполнены.", "ОК");
                    return;
                }

                if (!double.TryParse(rawStockSelf, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double stockSelf) || stockSelf <= 0)
                {
                    await DisplayAlert("Ошибка", "Введите корректный запас препарата (мг).", "ОК");
                    return;
                }

                int daysAvailable = maxDailyDose > 0 ? (int)Math.Floor(stockSelf / maxDailyDose) : 0;

                var med = new Medicine
                {
                    Name = name,
                    Unit = "мг",
                    IntakesPerDay = intakesPerDaySelf,
                    IntakeTimes = times.ToArray(),
                    IntakeDoses = doses.ToArray(),
                    StockAmount = stockSelf,
                    PackageCount = (int)Math.Floor(stockSelf), 
                    MaxDailyDosePerKg = maxPerKg,
                    DaysAvailable = daysAvailable,
                    SpecialInstructions = specialInstructions
                };

                await App.Database.SaveMedicineAsync(med);
                ScheduleMedicineNotifications(med.Name, times);
                await Navigation.PopAsync();

            }


        }

        // Генерация локальных уведомлений
        private void ScheduleMedicineNotifications(string medicineName, List<string> intakeTimes)
        {
            int id = 1000;
            foreach (var t in intakeTimes)
            {
                if (TimeSpan.TryParse(t, out var time))
                {
                    var now = DateTime.Now;
                    var notifyAt = DateTime.Today.Add(time);
                    if (notifyAt <= now)
                        notifyAt = notifyAt.AddDays(1);

                    var req = new NotificationRequest
                    {
                        NotificationId = id++,
                        Title = "Приём лекарства",
                        Description = $"Пора принять {medicineName}",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = notifyAt,
                            RepeatType = NotificationRepeat.Daily
                        }
                    };
                    Plugin.LocalNotification.NotificationCenter.Current.Show(req);
                }
            }
        }
    }
}
