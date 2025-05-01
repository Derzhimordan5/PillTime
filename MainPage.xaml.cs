namespace PillTime;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        // Переходим на AppShell (TabbedPage)
        Application.Current.MainPage = new AppShell();
    }
}
