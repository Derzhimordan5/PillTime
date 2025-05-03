namespace PillTime.Views
{
    public partial class UserProfilePage : ContentPage
    {
        // Диапазоны для Picker-ов
        private readonly List<string> _ages = Enumerable.Range(1, 120)
                                                         .Select(i => i.ToString())
                                                         .ToList();
        private readonly List<string> _weights = Enumerable.Range(1, 200)
                                                            .Select(i => i.ToString())
                                                            .ToList();

        private bool _isEditing;

        public UserProfilePage(bool isEditing = false)
        {
            InitializeComponent();
            _isEditing = isEditing;

            // Привязываем диапазоны к Picker-ам
            AgePicker.ItemsSource = _ages;
            WeightPicker.ItemsSource = _weights;

            if (_isEditing)
                LoadUserProfile();
        }

        void LoadUserProfile()
        {
            // Пол
            var savedGender = Preferences.Get("Gender", string.Empty);
            if (!string.IsNullOrEmpty(savedGender))
                GenderPicker.SelectedItem = savedGender;

            // Возраст
            var age = Preferences.Get("Age", 1);
            AgePicker.SelectedItem = age.ToString();

            // Вес
            var weight = Preferences.Get("Weight", 1.0);
            WeightPicker.SelectedItem = ((int)weight).ToString();
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            // Пол
            if (GenderPicker.SelectedItem is not string gender)
            {
                await DisplayAlert("Ошибка", "Выберите пол", "ОК");
                return;
            }

            // Возраст
            if (AgePicker.SelectedItem is not string ageText
             || !int.TryParse(ageText, out var age))
            {
                await DisplayAlert("Ошибка", "Выберите корректный возраст", "ОК");
                return;
            }

            // Вес
            if (WeightPicker.SelectedItem is not string weightText
             || !double.TryParse(weightText, out var weight))
            {
                await DisplayAlert("Ошибка", "Выберите корректный вес", "ОК");
                return;
            }

            // Сохраняем
            Preferences.Set("Gender", gender);
            Preferences.Set("Age", age);
            Preferences.Set("Weight", weight);

            // Переход в основное приложение
            Application.Current.MainPage = new MainTabbedPage();
        }
    }
}
