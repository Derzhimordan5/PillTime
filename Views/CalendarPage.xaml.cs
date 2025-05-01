namespace PillTime.Views;
using PillTime.Models;
public partial class CalendarPage : ContentPage
{
 



    private async void OnMedicineCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is Medicine medicine)
        {
            medicine.IsTakenToday = e.Value;
            await App.Database.SaveMedicineAsync(medicine);
        }
    }

}
