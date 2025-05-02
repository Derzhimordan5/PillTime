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
            MainPage = new Views.MainTabbedPage();
        }
        else
        {
            MainPage = new Views.UserProfilePage();
        }

        // 💥 Запускаем сброс отметок и пересчёт дней
        ResetMedicinesForNewDay();
    }

    private async void ResetMedicinesForNewDay()
    {
        var today = DateTime.Today.ToString("yyyy-MM-dd");
        var lastResetDate = Preferences.Get("LastResetDate", "");

        if (lastResetDate == today)
        {
            // Уже сбрасывали сегодня — ничего не делаем
            return;
        }

        // Обновляем дату сброса
        Preferences.Set("LastResetDate", today);

        var medicines = await Database.GetMedicinesAsync();
        foreach (var med in medicines)
        {
            med.IsTakenToday = false;
            med.RecalculateDaysAvailable();
            await Database.SaveMedicineAsync(med);
        }
    }
}
