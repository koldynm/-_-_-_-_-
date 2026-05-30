using Supabase.Gotrue;
using System.Windows;
using мне_бы_жить_в_шоколаде.Entities;
using мне_бы_жить_в_шоколаде.Pages;

namespace мне_бы_жить_в_шоколаде
{
    
    public partial class MainWindow : Window
    {
        private Supabase.Client _supabase;
        private Session _session;
        private Pages.RequestsList requestsList;
        private Profile? currentProfile;


        public MainWindow(Supabase.Client supabase, Session session)
        {
            InitializeComponent();
            _supabase = supabase;
            _session = session;
            init();
        }

        private async void init()
        {

            await ListenUser();

            requestsList = new Pages.RequestsList(
                _supabase, 
                currentProfile, 
                (r => 
            {
                var Page = new EditRequest(r, _supabase, (u =>
                {
                    NavigationHost.Navigate(requestsList);
                    if (u) requestsList.LoadData();
                }));
                NavigationHost.Navigate(Page);
            }),
                (r =>
                {
                    var page = new RequestControl(_supabase, r, currentProfile, (() =>
                    {
                        NavigationHost.Navigate(requestsList);
                        requestsList.LoadData();
                    }));
                    NavigationHost.Navigate(page);
                }));
            NavigationHost.Navigate(requestsList);
        }


        private async Task ListenUser()
        {

            try
            {
                currentProfile = await _supabase
                    .From<Profile>() 
                    .Filter("id", Postgrest.Constants.Operator.Equals, _session.User.Id)
                    .Single();
               

                if (currentProfile == null)
                {
                    throw new Exception("qwrtyuiop");
                }

                NameText.Text = currentProfile.Name;
                string roleName = "";
                switch (currentProfile.Role)
                {
                    case "requster":
                        roleName = "Пользователь";
                        break;
                    case "technician":
                        roleName = "Техник";
                        break;
                    case "admin":
                        roleName = "Администратор";
                        break;
                }

                RoleText.Text = roleName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}");
            }
        }


        private void BtnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            OpenRequests.IsChecked = false;
            AcceptedRequests.IsChecked = false;
            ArchivedRequests.IsChecked = false;

            var addPage = new Pages.AddRequest(_supabase, (updated =>
            {
                requestsList.LoadData();
                NavigationHost.Navigate(requestsList);
                OpenRequests.IsChecked = true;
            }));
            NavigationHost.Navigate(addPage);
        }

        private void AcceptedRequests_Click(object sender, RoutedEventArgs e)
        {
            requestsList.SetFilterStatus("in_progress");
        }

        private void OpenRequests_Click(object sender, RoutedEventArgs e)
        {
            requestsList.SetFilterStatus("new");
        }

        private void ArchivedRequests_Click(object sender, RoutedEventArgs e)
        {

            requestsList.SetFilterStatus("closed");
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