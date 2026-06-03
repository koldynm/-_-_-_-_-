using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде.Pages;

public partial class EquipmentsControl : Page
{
    private readonly Supabase.Client _supabase;

    public EquipmentsControl(Supabase.Client supabase)
    {
        InitializeComponent();
        _supabase = supabase;
        LoadEquipments();
    }

    private async void LoadEquipments()
    {
        try
        {
            var response = await _supabase
                .From<Equipment>()
                .Order("inventory_number", Postgrest.Constants.Ordering.Ascending)
                .Get();

            EquipmentsGrid.ItemsSource = response.Models;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки оборудования: {ex.Message}");
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadEquipments();
    }

    private void EquipmentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EquipmentsGrid.SelectedItem is Equipment equipment)
        {
            StatusCombo.SelectedValue = equipment.Status;
        }
    }

    private async void SaveStatus_Click(object sender, RoutedEventArgs e)
    {
        if (EquipmentsGrid.SelectedItem is not Equipment equipment)
        {
            MessageBox.Show("Выберите оборудование.");
            return;
        }

        if (StatusCombo.SelectedValue is not string status)
        {
            MessageBox.Show("Выберите статус.");
            return;
        }

        try
        {
            await _supabase.From<Equipment>()
                .Where(item => item.Id == equipment.Id)
                .Set(item => item.Status, status)
                .Update();

            LoadEquipments();
            MessageBox.Show("Статус оборудования обновлен.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения статуса: {ex.Message}");
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (EquipmentsGrid.SelectedItem is not Equipment equipment)
        {
            MessageBox.Show("Выберите оборудование.");
            return;
        }

        if (MessageBox.Show("Удалить выбранное оборудование?", "Подтверждение", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            await _supabase.From<Equipment>()
                .Where(item => item.Id == equipment.Id)
                .Delete();

            LoadEquipments();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления оборудования: {ex.Message}");
        }
    }
}
