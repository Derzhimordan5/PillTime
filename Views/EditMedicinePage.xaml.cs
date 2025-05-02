using PillTime.Models;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;

namespace PillTime.Views
{
    public partial class EditMedicinePage : ContentPage
    {
        private Medicine _medicine;
        private readonly List<string> _units = new() { "ампула", "грамм", "капля", "капсула", "пакетик", "таблетка" };

        public EditMedicinePage(Medicine medicine)
        {
            InitializeComponent();
            _medicine = medicine;
            DoctorUnitPicker.ItemsSource = _units;

            LoadMedicineData();
        }

        private void LoadMedicineData()
        {
            NameEntry.Text = _medicine.Name;
            ModePicker.SelectedIndex = _medicine.Unit == "мг" ? 1 : 0;
            DoctorUnitPicker.SelectedItem = _medicine.Unit;
            IntakesPerDayEntry.Text = _medicine.IntakesPerDay.ToString();
            IntakesPerDaySelfEntry.Text = _medicine.IntakesPerDay.ToString();
            StockEntry.Text = _medicine.StockAmount.ToString();
            StockSelfEntry.Text = _medicine.StockAmount.ToString();
            MaxDailyDosePerKgEntry.Text = _medicine.MaxDailyDosePerKg.ToString();
            SpecialInstructionsEntry.Text = _medicine.SpecialInstructions;

            // показываем блоки
            OnModeChanged(null, null);

            // заполняем расписания
            if (_medicine.Unit == "мг")
            {
                OnIntakesPerDaySelfChanged(null, null);
            }
            else
            {
                OnIntakesPerDayChanged(null, null);
            }
        }

        private void OnModeChanged(object sender, EventArgs e)
        {
            bool isDoctor = ModePicker.SelectedIndex == 0;
            DoctorBlock.IsVisible = isDoctor;
            SelfBlock.IsVisible = !isDoctor;
        }

        private void OnIntakesPerDayChanged(object sender, EventArgs e)
        {
            DoctorScheduleStack.Children.Clear();

            if (!int.TryParse(IntakesPerDayEntry.Text, out int count) || count <= 0)
                return;

            var doses = _medicine.IntakeDoses;
            var times = _medicine.IntakeTimes;

            for (int i = 0; i < count; i++)
            {
                var timePicker = new TimePicker
                {
                    Time = TimeSpan.TryParse(times.Length > i ? times[i] : "08:00", out var t) ? t : new TimeSpan(8, 0, 0),
                    Format = "HH:mm"
                };
                var doseEntry = new Entry
                {
                    Keyboard = Keyboard.Numeric,
                    Text = doses.Length > i ? doses[i].Split(' ')[0] : "",
                    Placeholder = $"{i + 1}-я доза"
                };

                var row = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Label { Text = $"Приём {i + 1}:", VerticalTextAlignment = TextAlignment.Center },
                        timePicker,
                        doseEntry
                    }
                };
                DoctorScheduleStack.Children.Add(row);
            }
        }

        private void OnIntakesPerDaySelfChanged(object sender, EventArgs e)
        {
            SelfScheduleStack.Children.Clear();

            if (!int.TryParse(IntakesPerDaySelfEntry.Text, out int count) || count <= 0)
                return;

            double.TryParse(MaxDailyDosePerKgEntry.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double maxPerKg);
            double weight = Preferences.Get("Weight", 0.0);
            double maxDailyDose = weight * maxPerKg;
            double dosePerIntake = count > 0 ? maxDailyDose / count : 0;

            var times = _medicine.IntakeTimes;

            for (int i = 0; i < count; i++)
            {
                var timePicker = new TimePicker
                {
                    Time = TimeSpan.TryParse(times.Length > i ? times[i] : "08:00", out var t) ? t : new TimeSpan(8, 0, 0),
                    Format = "HH:mm"
                };
                var doseLabel = new Label
                {
                    Text = $"{dosePerIntake:F2} мг",
                    VerticalTextAlignment = TextAlignment.Center
                };

                var row = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children =
                    {
                        new Label { Text = $"Приём {i + 1}:", VerticalTextAlignment = TextAlignment.Center },
                        timePicker,
                        doseLabel
                    }
                };
                SelfScheduleStack.Children.Add(row);
            }
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            _medicine.Name = NameEntry.Text?.Trim();
            _medicine.SpecialInstructions = SpecialInstructionsEntry.Text?.Trim();


            if (ModePicker.SelectedIndex == 0)
            {
                _medicine.Unit = DoctorUnitPicker.SelectedItem as string;
                _medicine.IntakesPerDay = int.TryParse(IntakesPerDayEntry.Text, out var ipd) ? ipd : 0;
                


                var times = new List<string>();
                var doses = new List<string>();

                foreach (var child in DoctorScheduleStack.Children)
                {
                    if (child is HorizontalStackLayout row &&
                        row.Children[1] is TimePicker tp &&
                        row.Children[2] is Entry de)
                    {
                        times.Add(tp.Time.ToString(@"hh\:mm"));
                        doses.Add($"{de.Text} {_medicine.Unit}");
                    }
                }

                _medicine.IntakeTimes = times.ToArray();
                _medicine.IntakeDoses = doses.ToArray();
                _medicine.StockAmount = double.TryParse(StockEntry.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var stock) ? stock : 0;
            }
            else
            {
                _medicine.Unit = "мг";
                _medicine.IntakesPerDay = int.TryParse(IntakesPerDaySelfEntry.Text, out var ipd) ? ipd : 0;
                _medicine.MaxDailyDosePerKg = double.TryParse(MaxDailyDosePerKgEntry.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var maxPerKg) ? maxPerKg : 0;
                


                var times = new List<string>();
                var doses = new List<string>();

                foreach (var child in SelfScheduleStack.Children)
                {
                    if (child is HorizontalStackLayout row && row.Children[1] is TimePicker tp)
                    {
                        times.Add(tp.Time.ToString(@"hh\:mm"));
                        double weight = Preferences.Get("Weight", 0.0);
                        double maxDailyDose = weight * _medicine.MaxDailyDosePerKg;
                        double dosePerIntake = maxDailyDose / _medicine.IntakesPerDay;
                        doses.Add($"{dosePerIntake:F2} мг");
                    }
                }

                _medicine.IntakeTimes = times.ToArray();
                _medicine.IntakeDoses = doses.ToArray();
                _medicine.StockAmount = double.TryParse(StockSelfEntry.Text?.Replace(',', '.'), System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out var stock) ? stock : 0;
            }

            _medicine.RecalculateDaysAvailable();
            await App.Database.SaveMedicineAsync(_medicine);
            await DisplayAlert("Сохранено", "Лекарство обновлено", "ОК");
            await Navigation.PopAsync();
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Удалить", "Вы уверены, что хотите удалить это лекарство?", "Да", "Нет");
            if (confirm)
            {
                await App.Database.DeleteMedicineAsync(_medicine);
                await DisplayAlert("Удалено", "Лекарство удалено", "ОК");
                await Navigation.PopAsync();
            }
        }
    }
}
