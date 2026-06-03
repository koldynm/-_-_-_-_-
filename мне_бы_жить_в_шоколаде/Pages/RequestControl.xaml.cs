using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Converters;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде.Pages;

public partial class RequestControl : Page
{
    private readonly Supabase.Client _supabase;
    private readonly RepairRequest _currentRequest;
    private readonly Profile _profile;
    private readonly Action _close;

    public RequestControl(Supabase.Client supabase, RepairRequest request, Profile profile, Action close)
    {
        InitializeComponent();
        _supabase = supabase;
        _currentRequest = request;
        _profile = profile;
        _close = close;

        UpdateActionButtons();
        LoadRequestDetails();
        LoadMessages();
    }

    private void UpdateActionButtons()
    {
        var showTechnicianActions = AppRoles.IsTechnician(_profile.Role) && _currentRequest.CompletedAt == null;
        BtnComplete.Visibility = showTechnicianActions ? Visibility.Visible : Visibility.Collapsed;
        BtnRefuse.Visibility = showTechnicianActions ? Visibility.Visible : Visibility.Collapsed;
    }

    private async void LoadRequestDetails()
    {
        RequestInfo.DataContext = _currentRequest;

        TxtRequester.Text = "Загрузка...";
        TxtTechnician.Text = _currentRequest.TechnicianId.HasValue ? "Загрузка..." : "Не назначен";
        TxtEquipment.Text = "Загрузка...";
        TxtEquipmentInfo.Text = string.Empty;

        if (_currentRequest.CompletedAt.HasValue)
        {
            TxtCompletedInfo.Visibility = Visibility.Visible;
            TxtCompletedInfo.Text = $"Завершена: {_currentRequest.CompletedAt.Value:f}";
        }
        else
        {
            TxtCompletedInfo.Visibility = Visibility.Collapsed;
        }

        UpdateActionButtons();
        await LoadAdditionalRequestInfoAsync();
    }

    private async Task LoadAdditionalRequestInfoAsync()
    {
        try
        {
            var requesterTask = LoadProfileNameAsync(_currentRequest.RequesterId);
            var technicianTask = _currentRequest.TechnicianId.HasValue
                ? LoadProfileNameAsync(_currentRequest.TechnicianId.Value)
                : Task.FromResult("Не назначен");
            var equipmentTask = LoadEquipmentAsync(_currentRequest.EquipmentId);

            await Task.WhenAll(requesterTask, technicianTask, equipmentTask);

            TxtRequester.Text = requesterTask.Result;
            TxtTechnician.Text = technicianTask.Result;

            var equipment = equipmentTask.Result;
            TxtEquipment.Text = GetEquipmentDisplayName(equipment);
            TxtEquipmentInfo.Text = GetEquipmentInfo(equipment);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки информации по заявке: {ex.Message}");
        }
    }

    private async Task<string> LoadProfileNameAsync(Guid profileId)
    {
        var profile = await _supabase
            .From<Profile>()
            .Filter("id", Postgrest.Constants.Operator.Equals, profileId.ToString())
            .Single();

        return profile?.Name ?? "Не найден";
    }

    private async Task<Equipment?> LoadEquipmentAsync(Guid equipmentId)
    {
        return await _supabase
            .From<Equipment>()
            .Filter("id", Postgrest.Constants.Operator.Equals, equipmentId.ToString())
            .Single();
    }

    private static string GetEquipmentDisplayName(Equipment? equipment)
    {
        if (equipment == null)
        {
            return "Оборудование не найдено";
        }

        return string.IsNullOrWhiteSpace(equipment.FullDisplayName)
            ? equipment.Model
            : equipment.FullDisplayName;
    }

    private static string GetEquipmentInfo(Equipment? equipment)
    {
        if (equipment == null)
        {
            return string.Empty;
        }

        var values = new[]
        {
            string.IsNullOrWhiteSpace(equipment.InventoryNumber) ? null : $"Инв. №: {equipment.InventoryNumber}",
            string.IsNullOrWhiteSpace(equipment.Status) ? null : $"Статус: {new EquipmentStatusConverter().Convert(equipment.Status, typeof(string), null, CultureInfo.CurrentCulture)}"
        };

        return string.Join(" · ", values.Where(value => value != null));
    }

    private async void LoadMessages()
    {
        try
        {
            var response = await _supabase.From<ChatMessage>()
                .Filter("request_id", Postgrest.Constants.Operator.Equals, _currentRequest.Id.ToString())
                .Order("created_at", Postgrest.Constants.Ordering.Ascending)
                .Get();

            var messages = response.Models.Select(msg => new UiChatMessage
            {
                Id = msg.Id,
                RequestId = msg.RequestId,
                SenderId = msg.SenderId,
                MessageText = msg.MessageText,
                CreatedAt = msg.CreatedAt,
                IsOwnMessage = msg.SenderId == _profile.Id
            }).ToList();

            MessagesList.ItemsSource = messages;
            ChatScroller.ScrollToEnd();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка загрузки чата: " + ex.Message);
        }
    }

    private async void BtnSendMessage_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtNewMessage.Text))
        {
            return;
        }

        var msg = new ChatMessage
        {
            RequestId = _currentRequest.Id,
            SenderId = _profile.Id,
            MessageText = TxtNewMessage.Text.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _supabase.From<ChatMessage>().Insert(msg);
        TxtNewMessage.Clear();
        LoadMessages();
    }

    private async void BtnComplete_Click(object sender, RoutedEventArgs e)
    {
        var now = DateTime.UtcNow;
        await _supabase.From<RepairRequest>()
            .Where(x => x.Id == _currentRequest.Id)
            .Set(x => x.CompletedAt, now)
            .Set(x => x.Status, "closed")
            .Update();

        _currentRequest.CompletedAt = now;
        _currentRequest.Status = "closed";
        LoadRequestDetails();
        MessageBox.Show("Заявка успешно завершена!");
        _close();
    }

    private async void BtnRefuse_OnClick(object sender, RoutedEventArgs e)
    {
        await _supabase.From<RepairRequest>()
            .Where(r => r.Id == _currentRequest.Id)
            .Set(r => r.TechnicianId, null)
            .Set(r => r.Status, "new")
            .Update();

        _close();
    }
}
