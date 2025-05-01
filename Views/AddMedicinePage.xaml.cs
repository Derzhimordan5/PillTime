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
            if (ModePicker.SelectedIndex == 0) // �� ���������� �����
            {
                DoctorBlock.IsVisible = true;
                SelfBlock.IsVisible = false;
            }
            else if (ModePicker.SelectedIndex == 1) // ��������������� ����
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
                await DisplayAlert("������", "�������� ������ ���������� ���������.", "��");
                return;
            }

            string name = NameEntry.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                await DisplayAlert("������", "������� �������� ���������.", "��");
                return;
            }

            if (ModePicker.SelectedIndex == 0) // �� ���������� �����
            {
                if (int.TryParse(PackageCountEntry.Text, out int packageCount) &&
                    int.TryParse(IntakesPerDayEntry.Text, out int intakesPerDay))
                {
                    // �������� ����� ������ �� DoctorTimesStack
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
                    await DisplayAlert("������", "������� ���������� �������� �������� ��� �������� � ������ � ����.", "��");
                }
            }
            else if (ModePicker.SelectedIndex == 1) // ��������������� ����
            {
                if (double.TryParse(DailyDosePerKgEntry.Text, out double dailyDosePerKg) &&
                    double.TryParse(TotalAmountEntry.Text, out double totalAmount) &&
                    int.TryParse(IntakesPerDaySelfEntry.Text, out int intakesPerDaySelf))
                {
                    double weight = Preferences.Get("Weight", 0.0);
                    if (weight == 0.0)
                    {
                        await DisplayAlert("������", "�� ������ ��� �������� � �������.", "��");
                        return;
                    }

                    double dailyDose = weight * dailyDosePerKg;
                    double days = totalAmount / dailyDose;

                    // �������� ����� ������ �� SelfTimesStack
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
                        DosePerIntake = $"{dailyDose / intakesPerDaySelf:F2} �� �� ����",
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
                    await DisplayAlert("������", "������� ���������� �������� �������� ��� �����.", "��");
                }
            }
        }

        private void ScheduleMedicineNotifications(string medicineName, List<string> intakeTimes)
        {
            int notificationId = 1000; // �������� � ������-�� ID

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
                        Title = "����������� � ����� ���������",
                        Description = $"���� ������� {medicineName}",
                        Schedule = new NotificationRequestSchedule
                        {
                            NotifyTime = notifyTime,
                            RepeatType = NotificationRepeat.Daily // ��������� ������ ����
                        }
                    };

                    Plugin.LocalNotification.NotificationCenter.Current.Show(request);
                }
            }
        }
    }
}
