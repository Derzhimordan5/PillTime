using PillTime.Models;

namespace PillTime.Views;

public partial class UserProfilePage : ContentPage
{
    private bool _isEditing = false;

    public UserProfilePage(bool isEditing = false)
    {
        InitializeComponent();
        _isEditing = isEditing;

        if (_isEditing)
            LoadUserProfile();
    }

    private void LoadUserProfile()
    {
        GenderEntry.Text = Preferences.Get("Gender", string.Empty);
        AgeEntry.Text = Preferences.Get("Age", 0).ToString();
        WeightEntry.Text = Preferences.Get("Weight", 0.0).ToString();
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        var gender = GenderEntry.Text;
        var ageText = AgeEntry.Text;
        var weightText = WeightEntry.Text;

        if (string.IsNullOrWhiteSpace(gender) || string.IsNullOrWhiteSpace(ageText) || string.IsNullOrWhiteSpace(weightText))
        {
            await DisplayAlert("Ошибка", "Пожалуйста, заполните все поля", "ОК");
            return;
        }

        if (int.TryParse(ageText, out int age) && double.TryParse(weightText, out double weight))
        {
            var profile = new UserProfile
            {
                Gender = gender,
                Age = age,
                Weight = weight
            };

            // Сохраняем профиль локально
            Preferences.Set("Gender", profile.Gender);
            Preferences.Set("Age", profile.Age);
            Preferences.Set("Weight", profile.Weight);

            // Переход в основное приложение
            Application.Current.MainPage = new Views.MainTabbedPage();
        }
        else
        {
            await DisplayAlert("Ошибка", "Возраст и вес должны быть числами", "ОК");
        }
    }
}
