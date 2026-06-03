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
        LoadProfiles();
    }

    private async void LoadProfiles()
    {
        try
        {
            var response = await _supabase
                .From<Profile>()
                .Order("full_name", Postgrest.Constants.Ordering.Ascending)
                .Get();

            _profiles = response.Models.Select(ProfileRow.FromProfile).ToList();
            UsersGrid.ItemsSource = _profiles;
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
            RoleCombo.SelectedValue = AppRoles.IsRequester(profile.Role)
                ? AppRoles.Requester
                : profile.Role;
        }
    }

    private async void SaveRole_Click(object sender, RoutedEventArgs e)
    {
        if (UsersGrid.SelectedItem is not ProfileRow profile)
        {
            MessageBox.Show("Выберите пользователя.");
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
                .Set(user => user.Role, role)
                .Set(user => user.UpdatedAt, DateTime.UtcNow)
                .Update();

            LoadProfiles();
            MessageBox.Show("Роль пользователя обновлена.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения роли: {ex.Message}");
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
