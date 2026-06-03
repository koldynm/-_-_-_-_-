using Supabase.Gotrue;
using System.Windows;
using мне_бы_жить_в_шоколаде.Entities;
using мне_бы_жить_в_шоколаде.Pages;

namespace мне_бы_жить_в_шоколаде
{
    public partial class MainWindow : Window
    {
        private readonly Supabase.Client _supabase;
        private readonly Session _session;
        private RequestsList? _requestsList;
        private Profile? _currentProfile;

        public MainWindow(Supabase.Client supabase, Session session)
        {
            InitializeComponent();
            _supabase = supabase;
            _session = session;
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadCurrentUserAsync();

            if (_currentProfile == null)
            {
                return;
            }

            AdminPanel.Visibility = AppRoles.IsAdmin(_currentProfile.Role)
                ? Visibility.Visible
                : Visibility.Collapsed;

            _requestsList = CreateRequestsList();
            NavigationHost.Navigate(_requestsList);
        }

        private RequestsList CreateRequestsList()
        {
            return new RequestsList(
                _supabase,
                _currentProfile!,
                request => NavigateToEditRequest(request),
                request => NavigateToRequestControl(request));
        }

        private void NavigateToEditRequest(RepairRequest request)
        {
            var page = new EditRequest(request, _supabase, updated =>
            {
                NavigateToRequests();

                if (updated)
                {
                    _requestsList?.LoadData();
                }
            });

            NavigationHost.Navigate(page);
        }

        private void NavigateToRequestControl(RepairRequest request)
        {
            var page = new RequestControl(_supabase, request, _currentProfile!, () =>
            {
                NavigateToRequests();
                _requestsList?.LoadData();
            });

            NavigationHost.Navigate(page);
        }

        private void NavigateToRequests()
        {
            if (_requestsList == null)
            {
                return;
            }

            NavigationHost.Navigate(_requestsList);
        }

        private async Task LoadCurrentUserAsync()
        {
            try
            {
                _currentProfile = await _supabase
                    .From<Profile>()
                    .Filter("id", Postgrest.Constants.Operator.Equals, _session.User.Id)
                    .Single();

                if (_currentProfile == null)
                {
                    throw new InvalidOperationException("Профиль пользователя не найден.");
                }

                NameText.Text = _currentProfile.Name;
                RoleText.Text = AppRoles.ToDisplayName(_currentProfile.Role);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}");
            }
        }

        private void BtnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            ClearNavigationSelection();

            var addPage = new AddRequest(_supabase, updated =>
            {
                _requestsList?.LoadData();
                NavigateToRequests();
                OpenRequests.IsChecked = true;
            });

            NavigationHost.Navigate(addPage);
        }

        private void AcceptedRequests_Click(object sender, RoutedEventArgs e)
        {
            ShowRequestsByStatus("in_progress");
        }

        private void OpenRequests_Click(object sender, RoutedEventArgs e)
        {
            ShowRequestsByStatus("new");
        }

        private void ArchivedRequests_Click(object sender, RoutedEventArgs e)
        {
            ShowRequestsByStatus("closed");
        }

        private void ShowRequestsByStatus(string status)
        {
            NavigateToRequests();
            _requestsList?.SetFilterStatus(status);
        }

        private void UsersControl_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAdminAccess())
            {
                return;
            }

            NavigationHost.Navigate(new UsersControl(_supabase));
        }

        private void EquipmentsControl_Click(object sender, RoutedEventArgs e)
        {
            if (!EnsureAdminAccess())
            {
                return;
            }

            NavigationHost.Navigate(new EquipmentsControl(_supabase));
        }

        private bool EnsureAdminAccess()
        {
            if (AppRoles.IsAdmin(_currentProfile?.Role))
            {
                return true;
            }

            MessageBox.Show("Раздел доступен только администратору.");
            ClearNavigationSelection();
            OpenRequests.IsChecked = true;
            ShowRequestsByStatus("new");
            return false;
        }

        private void ClearNavigationSelection()
        {
            OpenRequests.IsChecked = false;
            AcceptedRequests.IsChecked = false;
            ArchivedRequests.IsChecked = false;
            UsersControl.IsChecked = false;
            EquipmentsControl.IsChecked = false;
        }

        private async void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _supabase.Auth.SignOut();

                var authWindow = new AuthWindow(_supabase);

                authWindow.Show();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка выхода: {ex.Message}");
            }
        }
    }
}
