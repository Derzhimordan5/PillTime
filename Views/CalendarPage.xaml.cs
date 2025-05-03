namespace PillTime.Views;
using PillTime.Models;
using System.Collections.ObjectModel;

public partial class CalendarPage : ContentPage
{
    public ObservableCollection<Medicine> TodayMedicines { get; set; } = new();

    public CalendarPage()
    {
        InitializeComponent();
        BindingContext = this;
        LoadTodayMedicines();
    }

    private async void OnMedicineCheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        if (sender is CheckBox checkBox && checkBox.BindingContext is Medicine med && e.Value)
        {
            double dose = med.PerDoseAmount;

            if (dose > 0)
            {
                med.StockAmount -= dose;
                if (med.StockAmount < 0)
                    med.StockAmount = 0;

                med.IsTakenToday = true;
                await App.Database.SaveMedicineAsync(med);

                await DisplayAlert("Отмечено", $"{med.Name}: -{dose} {med.Unit}, осталось {med.StockAmount}", "ОК");

                await LoadTodayMedicines();
            }
            else
            {
                await DisplayAlert("Ошибка", "Не удалось распознать дозу.", "ОК");
                checkBox.IsChecked = false;
            }
        }
    }

    private async Task LoadTodayMedicines()
    {
        var all = await App.Database.GetMedicinesAsync();
        var withStock = all
            .Where(m => m.StockAmount > 0)
            .ToList();

        TodayMedicines = new ObservableCollection<Medicine>(withStock);
        OnPropertyChanged(nameof(TodayMedicines));
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadTodayMedicines();
    }

}
