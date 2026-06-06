using System.Windows;
using мне_бы_жить_в_шоколаде.Entities;
using мне_бы_жить_в_шоколаде.Pages;

namespace мне_бы_жить_в_шоколаде
{
    public partial class MainWindow : Window
    {
        private RequestsList? _requestsList;

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var profile = await Globals.GetProfile();
            if (profile is null) return;

            UpdateProfileText(profile);

            AdminPanel.Visibility = AppRoles.IsAdmin(profile.Role)
                ? Visibility.Visible
                : Visibility.Collapsed;

            _requestsList = CreateRequestsList();
            NavigationHost.Navigate(_requestsList);
        }

        private RequestsList CreateRequestsList()
        {
            return new RequestsList(
                request => NavigateToEditRequest(request),
                request => NavigateToRequestControl(request)
            );
        }

        private void NavigateToEditRequest(RepairRequest request)
        {
            var page = new EditRequest(request, updated =>
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
            var page = new RequestControl(request, () =>
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

        private async void UpdateProfileText(Profile profile)
        {
            NameText.Text = profile.Name;
            RoleText.Text = AppRoles.ToDisplayName(profile.Role);
        }

        private void BtnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            ClearNavigationSelection();

            var addPage = new AddRequest(updated =>
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

        private async void UsersControl_Click(object sender, RoutedEventArgs e)
        {
            if (!await EnsureAdminAccess())
            {
                return;
            }

            NavigationHost.Navigate(new UsersControl());
        }

        private async void EquipmentsControl_Click(object sender, RoutedEventArgs e)
        {
            if (!await EnsureAdminAccess())
            {
                return;
            }

            NavigationHost.Navigate(new EquipmentsControl());
        }

        private async Task<bool> EnsureAdminAccess()
        {
            var profile = await Globals.GetProfile();
            if (AppRoles.IsAdmin(profile.Role))
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
                var client = await Globals.GetClient();
                await client.Auth.SignOut();

                var authWindow = new AuthWindow();

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
