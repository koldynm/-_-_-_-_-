using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде.Pages;

public partial class EquipmentsControl : Page
{
    private List<EquipmentRow> _equipments = [];
    private List<LookupOption> _types = [];
    private List<LookupOption> _locations = [];
    private bool _isCreateMode;

    public EquipmentsControl()
    {
        InitializeComponent();
        ClearEditor(false);
        LoadDictionariesAndEquipments();
    }

    private async void LoadDictionariesAndEquipments()
    {
        try
        {
            await LoadDictionaries();
            await LoadEquipmentsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки справочников и оборудования: {ex.Message}");
        }
    }

    private async Task LoadDictionaries()
    {
        var client = await Globals.GetClient();
        var typesResponse = await client
            .From<EquipmentType>()
            .Order("name", Postgrest.Constants.Ordering.Ascending)
            .Get();

        var locationsResponse = await client
            .From<Location>()
            .Order("building", Postgrest.Constants.Ordering.Ascending)
            .Get();

        _types = [LookupOption.Empty("Без типа"), .. typesResponse.Models.Select(type => new LookupOption(type.Id, type.Name))];
        _locations = [LookupOption.Empty("Без локации"), .. locationsResponse.Models.Select(location => new LookupOption(location.Id, FormatLocation(location)))];

        TypeCombo.ItemsSource = _types;
        LocationCombo.ItemsSource = _locations;
    }

    private async Task LoadEquipmentsAsync()
    {
        var client = await Globals.GetClient();

        var selectedId = (EquipmentsGrid.SelectedItem as EquipmentRow)?.Id;

        var response = await client
            .From<Equipment>()
            .Order("inventory_number", Postgrest.Constants.Ordering.Ascending)
            .Get();

        _equipments = response.Models
            .Select(equipment => EquipmentRow.FromEquipment(equipment, _types, _locations))
            .ToList();

        EquipmentsGrid.ItemsSource = _equipments;

        if (selectedId.HasValue && !_isCreateMode)
        {
            EquipmentsGrid.SelectedItem = _equipments.FirstOrDefault(equipment => equipment.Id == selectedId.Value);
        }

        if (EquipmentsGrid.SelectedItem is not EquipmentRow && !_isCreateMode)
        {
            ClearEditor(false);
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadDictionariesAndEquipments();
    }

    private void EquipmentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EquipmentsGrid.SelectedItem is EquipmentRow equipment)
        {
            _isCreateMode = false;
            FillEditor(equipment);
        }
        else if (!_isCreateMode)
        {
            ClearEditor(false);
        }
    }

    private void AddEquipment_Click(object sender, RoutedEventArgs e)
    {
        EquipmentsGrid.SelectedItem = null;
        ClearEditor(true);
    }

    private void FillEditor(EquipmentRow equipment)
    {
        ShowEditor();
        EquipmentEditorHintText.Text = "Измените данные оборудования и нажмите «Сохранить»";
        InventoryNumberText.Text = equipment.InventoryNumber;
        TypeCombo.SelectedValue = equipment.TypeId;
        ModelText.Text = equipment.Model;
        LocationCombo.SelectedValue = equipment.LocationId;
        StatusCombo.SelectedValue = equipment.Status;
        CreatedAtText.Text = equipment.CreatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        SetEditorEnabled(true);
    }

    private void ClearEditor(bool createMode)
    {
        _isCreateMode = createMode;

        if (!createMode)
        {
            ShowPlaceholder();
            return;
        }

        ShowEditor();
        EquipmentEditorHintText.Text = "Заполните данные нового оборудования";
        InventoryNumberText.Text = string.Empty;
        TypeCombo.SelectedIndex = 0;
        ModelText.Text = string.Empty;
        LocationCombo.SelectedIndex = 0;
        StatusCombo.SelectedValue = "in_stock";
        CreatedAtText.Text = string.Empty;
        SetEditorEnabled(true);
    }

    private void ShowEditor()
    {
        EquipmentPlaceholder.Visibility = Visibility.Collapsed;
        EquipmentEditorPanel.Visibility = Visibility.Visible;
    }

    private void ShowPlaceholder()
    {
        EquipmentPlaceholder.Visibility = Visibility.Visible;
        EquipmentEditorPanel.Visibility = Visibility.Collapsed;
        InventoryNumberText.Text = string.Empty;
        TypeCombo.SelectedIndex = -1;
        ModelText.Text = string.Empty;
        LocationCombo.SelectedIndex = -1;
        StatusCombo.SelectedIndex = -1;
        CreatedAtText.Text = string.Empty;
    }

    private void SetEditorEnabled(bool isEnabled)
    {
        InventoryNumberText.IsEnabled = isEnabled;
        TypeCombo.IsEnabled = isEnabled;
        ModelText.IsEnabled = isEnabled;
        LocationCombo.IsEnabled = isEnabled;
        StatusCombo.IsEnabled = isEnabled;
    }

    private void ClearEquipmentForm_Click(object sender, RoutedEventArgs e)
    {
        if (_isCreateMode)
        {
            ClearEditor(true);
            return;
        }

        if (EquipmentsGrid.SelectedItem is EquipmentRow equipment)
        {
            FillEditor(equipment);
        }
    }

    private async void SaveEquipment_Click(object sender, RoutedEventArgs e)
    {
        if (!_isCreateMode && EquipmentsGrid.SelectedItem is not EquipmentRow)
        {
            MessageBox.Show("Выберите оборудование или нажмите «Добавить».");
            return;
        }

        if (string.IsNullOrWhiteSpace(InventoryNumberText.Text))
        {
            MessageBox.Show("Укажите инвентарный номер.");
            return;
        }

        if (string.IsNullOrWhiteSpace(ModelText.Text))
        {
            MessageBox.Show("Укажите модель оборудования.");
            return;
        }

        if (StatusCombo.SelectedValue is not string status)
        {
            MessageBox.Show("Выберите статус.");
            return;
        }

        try
        {
            var client = await Globals.GetClient();

            var typeId = GetSelectedGuid(TypeCombo);
            var locationId = GetSelectedGuid(LocationCombo);

            if (_isCreateMode)
            {
                var newEquipment = new Equipment
                {
                    InventoryNumber = InventoryNumberText.Text.Trim(),
                    TypeId = typeId,
                    Model = ModelText.Text.Trim(),
                    LocationId = locationId,
                    Status = status,
                    CreatedAt = DateTime.UtcNow
                };

                await client.From<Equipment>().Insert(newEquipment);
                _isCreateMode = false;
                MessageBox.Show("Оборудование создано.");
            }
            else if (EquipmentsGrid.SelectedItem is EquipmentRow equipment)
            {
                await client.From<Equipment>()
                    .Where(item => item.Id == equipment.Id)
                    .Set(item => item.InventoryNumber, InventoryNumberText.Text.Trim())
                    .Set(item => item.TypeId, typeId)
                    .Set(item => item.Model, ModelText.Text.Trim())
                    .Set(item => item.LocationId, locationId)
                    .Set(item => item.Status, status)
                    .Update();

                MessageBox.Show("Оборудование обновлено.");
            }

            await LoadEquipmentsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения оборудования: {ex.Message}");
        }
    }

    private async void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (_isCreateMode)
        {
            ClearEditor(false);
            return;
        }

        if (EquipmentsGrid.SelectedItem is not EquipmentRow equipment)
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
            var client = await Globals.GetClient();
            await client.From<Equipment>()
                .Where(item => item.Id == equipment.Id)
                .Delete();

            EquipmentsGrid.SelectedItem = null;
            await LoadEquipmentsAsync();
            MessageBox.Show("Оборудование удалено.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления оборудования: {ex.Message}");
        }
    }

    private static Guid? GetSelectedGuid(ComboBox comboBox)
    {
        return comboBox.SelectedValue is Guid id ? id : null;
    }

    private static string FormatLocation(Location location)
    {
        var locationName = string.Join(", ", new[] { location.Building, location.RoomNumber }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(locationName) ? "Без названия" : locationName;
    }

    private sealed record LookupOption(Guid? Id, string Name)
    {
        public static LookupOption Empty(string name) => new(null, name);
    }

    private sealed class EquipmentRow
    {
        public Guid Id { get; init; }
        public string InventoryNumber { get; init; } = string.Empty;
        public Guid? TypeId { get; init; }
        public string TypeName { get; init; } = string.Empty;
        public string Model { get; init; } = string.Empty;
        public string SerialNumber { get; init; } = string.Empty;
        public Guid? LocationId { get; init; }
        public string LocationName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public string PhotoUrl { get; init; } = string.Empty;

        public static EquipmentRow FromEquipment(Equipment equipment, IEnumerable<LookupOption> types, IEnumerable<LookupOption> locations) => new()
        {
            Id = equipment.Id,
            InventoryNumber = equipment.InventoryNumber,
            TypeId = equipment.TypeId,
            TypeName = types.FirstOrDefault(type => type.Id == equipment.TypeId)?.Name ?? "Без типа",
            Model = equipment.Model,
            SerialNumber = equipment.SerialNumber,
            LocationId = equipment.LocationId,
            LocationName = locations.FirstOrDefault(location => location.Id == equipment.LocationId)?.Name ?? "Без локации",
            Status = equipment.Status,
            CreatedAt = equipment.CreatedAt,
            PhotoUrl = equipment.PhotoUrl
        };
    }
}
