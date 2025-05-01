using Microsoft.Maui.Controls;

namespace PillTime.Views;

public partial class MainTabbedPage : TabbedPage
{
    public MainTabbedPage()
    {
        InitializeComponent();
    }

    private async void OnProfileClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new UserProfilePage(true)); // Переход в режим редактирования
    }
}
