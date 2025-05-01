using PillTime.Models;
using Plugin.LocalNotification;

namespace PillTime.Views
{
    public partial class AddMedicinePage : ContentPage
    {
        public AddMedicinePage()
        {
            InitializeComponent();
        }

        private void OnModeChanged(object sender, EventArgs e)
        {
            if (ModePicker.SelectedIndex == 0) // По назначению врача
            {
                DoctorBlock.IsVisible = true;
                SelfBlock.IsVisible = false;
            }
            else if (ModePicker.SelectedIndex == 1) // Самостоятельный курс
            {
                DoctorBlock.IsVisible = false;
                SelfBlock.IsVisible = true;
            }
        }

        private void OnAddDoctorTimeClicked(object sender, EventArgs e)
        {
            var timePicker = new TimePicker
            {
                Time = new TimeSpan(8, 0, 0),
                Format = "HH:mm",
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                HeightRequest = 50
            };

            DoctorTimesStack.Children.Add(timePicker);
        }

        private void OnAddSelfTimeClicked(object sender, EventArgs e)
        {
            var timePicker = new TimePicker
            {
                Time = new TimeSpan(8, 0, 0),
                Format = "HH:mm",
                BackgroundColor = Colors.White,
                TextColor = Colors.Black,
                HeightRequest = 50
            };

            SelfTimesStack.Children.Add(timePicker);
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (ModePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Ошибка", "Выберите способ добавления лекарства.", "ОК");
                return;
            }

            string name = NameEntry.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("Ошибка", "Введите название лекарства.", "ОК");
                return;
            }

            if (ModePicker.SelectedIndex == 0) // По назначению врача
            {
                if (int.TryParse(PackageCountEntry.Text, out int packageCount) &&
                    int.TryParse(IntakesPerDayEntry.Text, out int intakesPerDay))
                {
                    // Собираем время приёмов из DoctorTimesStack
                    var intakeTimes = new List<string>();
                    foreach (var child in DoctorTimesStack.Children)
                    {
                        if (child is TimePicker tp)
                            intakeTimes.Add(tp.Time.ToString(@"hh\:mm"));
                    }
                    string timesPerDayString = string.Join(", ", intakeTimes);

                    var medicine = new Medicine
                    {
                        Name = name,
                        PackageCount = packageCount,
                        DosePerIntake = DosePerIntakeEntry.Text,
                        IntakesPerDay = intakesPerDay,
                        TimesPerDay = timesPerDayString,
                        SpecialInstructions = SpecialInstructionsEntry.Text
                    };

                    await App.Database.SaveMedicineAsync(medicine);
                    ScheduleMedicineNotifications(medicine.Name, intakeTimes);
                    await Navigation.PopAsync();

                }
                else
                {
                    await DisplayAlert("Ошибка", "Введите корректные числовые значения для упаковки и приёмов в день.", "ОК");
                }
            }
            else if (ModePicker.SelectedIndex == 1) // Самостоятельный курс
            {
                if (double.TryParse(DailyDosePerKgEntry.Text, out double dailyDosePerKg) &&
                    double.TryParse(TotalAmountEntry.Text, out double totalAmount) &&
                    int.TryParse(IntakesPerDaySelfEntry.Text, out int intakesPerDaySelf))
                {
                    double weight = Preferences.Get("Weight", 0.0);
                    if (weight == 0.0)
                    {
                        await DisplayAlert("Ошибка", "Не указан вес пациента в профиле.", "ОК");
                        return;
                    }

                    double dailyDose = weight * dailyDosePerKg;
                    double days = totalAmount / dailyDose;

                    // Собираем время приёмов из SelfTimesStack
                    var intakeTimes = new List<string>();
                    foreach (var child in SelfTimesStack.Children)
                    {
                        if (child is TimePicker tp)
                            intakeTimes.Add(tp.Time.ToString(@"hh\:mm"));
                    }
                    string timesPerDayString = string.Join(", ", intakeTimes);

                    var medicine = new Medicine
                    {
                        Name = name,
                        PackageCount = (int)Math.Floor(days),
                        DosePerIntake = $"{dailyDose / intakesPerDaySelf:F2} мг за приём",
                        IntakesPerDay = intakesPerDaySelf,
                        TimesPerDay = timesPerDayString,
                        SpecialInstructions = SpecialInstructionsSelfEntry.Text
                    };

                    await App.Database.SaveMedicineAsync(medicine);
                    ScheduleMedicineNotifications(medicine.Name, intakeTimes);
                    await Navigation.PopAsync();

                }
                else
                {
                    await DisplayAlert("Ошибка", "Введите корректные числовые значения для курса.", "ОК");
                }
            }
        }

        private void ScheduleMedicineNotifications(string medicineName, List<string> intakeTimes)
        {
            int notificationId = 1000; // Начинаем с какого-то ID

            foreach (var timeStr in intakeTimes)
            {
                if (TimeSpan.TryParse(timeStr, out TimeSpan time))
                {
                    DateTime now = DateTime.Now;
                    DateTime notifyTime = DateTime.Today.Add(time);

                    if (notifyTime <= now)
                    {
                        notifyTime = notifyTime.AddDays(1);
                    }

                    var request = new NotificationRequest
                    {
                        NotificationId = notificationId++,
                        Title = "Напоминание о приёме лекарства",
                        Description = $"Пора принять {medicineName}",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = notifyTime,
                            RepeatType = NotificationRepeat.Daily // Повторять каждый день
                        }
                    };

                    Plugin.LocalNotification.NotificationCenter.Current.Show(request);
                }
            }
        }
    }
}
