using PillTime.Models;
using System.Collections.ObjectModel;

namespace PillTime.Views;

public partial class MedListPage : ContentPage
{
    public ObservableCollection<Medicine> Medicines { get; set; } = new();

    public MedListPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        Medicines.Clear();
        var meds = await App.Database.GetMedicinesAsync();
        foreach (var med in meds)
        {
            Medicines.Add(med);
        }
    }

    private async Task ReloadMedicinesAsync()
    {
        Medicines.Clear();
        var meds = await App.Database.GetMedicinesAsync();
        foreach (var med in meds)
        {
            Medicines.Add(med);
        }
    }

    private async void OnNavigateToAddMedicinePage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new AddMedicinePage());
    }
    private async void OnMedicineSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Medicine selectedMedicine)
        {
            await Navigation.PushAsync(new EditMedicinePage(selectedMedicine));
            MedicinesCollection.SelectedItem = null;  
        }
    }





}
