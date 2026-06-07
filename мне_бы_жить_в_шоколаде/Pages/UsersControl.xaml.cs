using Supabase.Gotrue;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде.Pages;

public partial class UsersControl : Page
{
    private List<ProfileRow> _profiles = [];
    private bool _isCreateMode;

    public UsersControl()
    {
        InitializeComponent();
        ClearEditor(false);
        LoadProfiles();
    }

    private async void LoadProfiles()
    {
        try
        {
            var adminAuth = await Globals.GetAdminAuth();
            var client = await Globals.GetClient();

            var usersList = await adminAuth.ListUsers();
            var users = usersList?.Users;

            if (users is null) return;

            var userLookup = users
                .Where(u => Guid.TryParse(u.Id, out _))
                .ToDictionary(u => Guid.Parse(u.Id));

            var response = await client
                .From<Profile>()
                .Order("full_name", Postgrest.Constants.Ordering.Ascending)
                .Get();

            _profiles = response.Models
                .Select(profile => ProfileRow.FromProfile(profile, userLookup.GetValueOrDefault(profile.Id)))
                .ToList();
            UsersGrid.ItemsSource = _profiles;

            var selectedId = (UsersGrid.SelectedItem as ProfileRow)?.Id;
            if (selectedId.HasValue && !_isCreateMode)
            {
                UsersGrid.SelectedItem = _profiles.FirstOrDefault(profile => profile.Id == selectedId.Value);
            }

            if (UsersGrid.SelectedItem is not ProfileRow && !_isCreateMode)
            {
                ClearEditor(false);
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

    private void AddUser_Click(object sender, RoutedEventArgs e)
    {
        UsersGrid.SelectedItem = null;
        ClearEditor(true);
    }

    private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (UsersGrid.SelectedItem is ProfileRow profile)
        {
            _isCreateMode = false;
            FillEditor(profile);
        }
        else if (!_isCreateMode)
        {
            ClearEditor(false);
        }
    }

    private void FillEditor(ProfileRow profile)
    {
        ShowEditor();
        EditorHintText.Text = "Измените данные пользователя и нажмите «Сохранить»";
        EmailText.Text = profile.Email;
        PasswordText.Text = profile.Password;
        NameText.Text = profile.Name;
        RoleCombo.SelectedValue = AppRoles.IsRequester(profile.Role)
            ? AppRoles.Requester
            : profile.Role;
        UpdatedAtText.Text = profile.UpdatedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm");

        try
        {
            var currentUserId = Globals.CurrentUserId;
            if (currentUserId is null) return;

            DeleteUserBtn.Visibility = currentUserId == profile.Id ? Visibility.Collapsed : Visibility.Visible;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
            DeleteUserBtn.Visibility = Visibility.Collapsed;
        }
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
        EditorHintText.Text = "Заполните данные нового пользователя";
        EmailText.Text = string.Empty;
        PasswordText.Text = string.Empty;
        NameText.Text = string.Empty;
        RoleCombo.SelectedValue = AppRoles.Requester;
        UpdatedAtText.Text = "Будет заполнено при сохранении";

        DeleteUserBtn.Visibility = createMode ? Visibility.Collapsed : Visibility.Visible;
    }

    private void ShowEditor()
    {
        UserPlaceholder.Visibility = Visibility.Collapsed;
        UserEditorPanel.Visibility = Visibility.Visible;
    }

    private void ShowPlaceholder()
    {
        UserPlaceholder.Visibility = Visibility.Visible;
        UserEditorPanel.Visibility = Visibility.Collapsed;
        EmailText.Text = string.Empty;
        PasswordText.Text = string.Empty;
        NameText.Text = string.Empty;
        RoleCombo.SelectedIndex = -1;
        UpdatedAtText.Text = string.Empty;
    }

    private void ResetUserForm_Click(object sender, RoutedEventArgs e)
    {
        if (_isCreateMode)
        {
            ClearEditor(true);
            return;
        }

        if (UsersGrid.SelectedItem is ProfileRow profile)
        {
            FillEditor(profile);
        }
    }

    private async void SaveUser_Click(object sender, RoutedEventArgs e)
    {
        if (!_isCreateMode && UsersGrid.SelectedItem is not ProfileRow)
        {
            MessageBox.Show("Выберите пользователя или нажмите «Добавить».");
            return;
        }

        if (string.IsNullOrWhiteSpace(EmailText.Text))
        {
            MessageBox.Show("Укажите электронную почту.");
            return;
        }

        if (string.IsNullOrWhiteSpace(NameText.Text))
        {
            MessageBox.Show("Укажите имя пользователя.");
            return;
        }

        if (RoleCombo.SelectedValue is not string)
        {
            MessageBox.Show("Выберите роль.");
            return;
        }

        try
        {

            if (_isCreateMode)
            {
                await createUser();

                _isCreateMode = false;
                MessageBox.Show("Пользователь добавлен.");
            }
            else if (UsersGrid.SelectedItem is ProfileRow profile)
            {
                await updateUser(profile);

                MessageBox.Show("Профиль пользователя обновлен.");
            }

            LoadProfiles();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения пользователя: {ex.Message}");
        }
    }

    private async void DeleteUser_Click(object sender, RoutedEventArgs e)
    {
        if (_isCreateMode)
        {
            MessageBox.Show("Нельзя удалить пользователя при его создании. Используйте очистить форму");
            return;
        }
        if (UsersGrid.SelectedItem is not ProfileRow profile)
        {
            MessageBox.Show("Выберите пользователя");
            return;
        }
        try
        {
            var currentUserId = Globals.CurrentUserId;
            if (currentUserId is null) return;

            if (profile.Id == currentUserId)
            {
                MessageBox.Show("Нельзя удалить самого себя");
                return;
            }

            var adminAuth = await Globals.GetAdminAuth();
            await adminAuth.DeleteUser(profile.Id.ToString());

            LoadProfiles();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления пользователя: {ex.Message}");
        }
    }

    private async Task createUser()
    {
        if (RoleCombo.SelectedValue is not string role) return;

        var adminAuth = await Globals.GetAdminAuth();

        await adminAuth.CreateUser(
            email: EmailText.Text,
            password: PasswordText.Text,
            attributes: new AdminUserAttributes
            {
                EmailConfirm = true,
                UserMetadata = new Dictionary<string, object>
                {
                    { "full_name", NameText.Text.Trim() },
                    { "role", role }
                }
            }
        );
        System.Diagnostics.Debug.WriteLine($"create new user: email: '{EmailText.Text}'; password: '{PasswordText.Text}';");
    }

    private async Task updateUser(ProfileRow profile)
    {
        if (RoleCombo.SelectedValue is not string role) return;

        var client = await Globals.GetClient();
        var adminAuth = await Globals.GetAdminAuth();

        await client.From<Profile>()
            .Where(user => user.Id == profile.Id)
            .Set(user => user.Name, NameText.Text.Trim())
            .Set(user => user.Role, role)
            .Set(user => user.UpdatedAt, DateTime.UtcNow)
            .Update();

        var authAttributes = new AdminUserAttributes();
        bool authNeedsUpdate = false;

        if (!string.Equals(EmailText.Text.Trim(), profile.Email, StringComparison.OrdinalIgnoreCase))
        {
            authAttributes.Email = EmailText.Text.Trim();
            authNeedsUpdate = true;
        }

        if (!string.IsNullOrWhiteSpace(PasswordText.Text))
        {
            authAttributes.Password = PasswordText.Text;
            authNeedsUpdate = true;
        }

        if (authNeedsUpdate)
            await adminAuth.UpdateUserById(profile.Id.ToString(), authAttributes);
    }

    private sealed class ProfileRow
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Role { get; init; } = string.Empty;
        public string RoleDisplayName => AppRoles.ToDisplayName(Role);
        public DateTime UpdatedAt { get; init; }

        public static ProfileRow FromProfile(Profile profile, User? user) => new()
        {
            Id = profile.Id,
            Email = user?.Email ?? "no email",
            Password = string.Empty,
            Name = profile.Name,
            Role = profile.Role,
            UpdatedAt = profile.UpdatedAt
        };
    }
}
