using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде.Pages;

public partial class UsersControl : Page
{
    private readonly Supabase.Client _supabase;
    private List<ProfileRow> _profiles = [];

    public UsersControl(Supabase.Client supabase)
    {
        InitializeComponent();
        _supabase = supabase;
        ClearEditor();
        LoadProfiles();
    }

    private async void LoadProfiles()
    {
        try
        {
            var selectedId = (UsersGrid.SelectedItem as ProfileRow)?.Id;
            var response = await _supabase
                .From<Profile>()
                .Order("full_name", Postgrest.Constants.Ordering.Ascending)
                .Get();

            _profiles = response.Models.Select(ProfileRow.FromProfile).ToList();
            UsersGrid.ItemsSource = _profiles;

            if (selectedId.HasValue)
            {
                UsersGrid.SelectedItem = _profiles.FirstOrDefault(profile => profile.Id == selectedId.Value);
            }

            if (UsersGrid.SelectedItem is not ProfileRow)
            {
                ClearEditor();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
        }
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        LoadProfiles();
    }

    private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UsersGrid.SelectedItem is ProfileRow profile)
        {
            FillEditor(profile);
        }
        else
        {
            ClearEditor();
        }
    }

    private void FillEditor(ProfileRow profile)
    {
        EditorHintText.Text = "Измените данные пользователя и нажмите «Сохранить»";
        UserIdText.Text = profile.Id.ToString();
        NameText.Text = profile.Name;
        RoleCombo.SelectedValue = AppRoles.IsRequester(profile.Role)
            ? AppRoles.Requester
            : profile.Role;
        UpdatedAtText.Text = profile.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");
        SetEditorEnabled(true);
    }

    private void ClearEditor()
    {
        EditorHintText.Text = "Выберите пользователя в таблице слева";
        UserIdText.Text = string.Empty;
        NameText.Text = string.Empty;
        RoleCombo.SelectedIndex = -1;
        UpdatedAtText.Text = string.Empty;
        SetEditorEnabled(false);
    }

    private void SetEditorEnabled(bool isEnabled)
    {
        NameText.IsEnabled = isEnabled;
        RoleCombo.IsEnabled = isEnabled;
    }

    private void ResetUserForm_Click(object sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is ProfileRow profile)
        {
            FillEditor(profile);
        }
    }

    private async void SaveUser_Click(object sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not ProfileRow profile)
        {
            MessageBox.Show("Выберите пользователя.");
            return;
        }

        if (string.IsNullOrWhiteSpace(NameText.Text))
        {
            MessageBox.Show("Укажите имя пользователя.");
            return;
        }

        if (RoleCombo.SelectedValue is not string role)
        {
            MessageBox.Show("Выберите роль.");
            return;
        }

        try
        {
            await _supabase.From<Profile>()
                .Where(user => user.Id == profile.Id)
                .Set(user => user.Name, NameText.Text.Trim())
                .Set(user => user.Role, role)
                .Set(user => user.UpdatedAt, DateTime.UtcNow)
                .Update();

            LoadProfiles();
            MessageBox.Show("Профиль пользователя обновлен.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения пользователя: {ex.Message}");
        }
    }

    private sealed class ProfileRow
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public string RoleDisplayName => AppRoles.ToDisplayName(Role);
        public DateTime UpdatedAt { get; init; }

        public static ProfileRow FromProfile(Profile profile) => new()
        {
            Id = profile.Id,
            Name = profile.Name,
            Role = profile.Role,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
