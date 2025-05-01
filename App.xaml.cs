namespace PillTime;

public partial class App : Application
{
    public static Services.PillTimeDatabase Database { get; private set; }

    public App()
    {
        InitializeComponent();

        string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "pilltime.db3");
        Database = new Services.PillTimeDatabase(dbPath);

        if (Preferences.ContainsKey("Weight"))
        {
            // Если данные уже есть — сразу идём в приложение
            MainPage = new Views.MainTabbedPage();
        }
        else
        {
            // Иначе показываем заполнение профиля
            MainPage = new Views.UserProfilePage();
        }
    }

}
