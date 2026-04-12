using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using мне_бы_жить_в_шоколаде.Entities;
using мне_бы_жить_в_шоколаде.Pages;

namespace мне_бы_жить_в_шоколаде
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Supabase.Client _supabase;
        private Pages.RequestsList requestsList;
        private Profile? currentProfile;


        public MainWindow()
        {
            InitializeComponent();
            init();
        }

        private async void init()
        {
            // 1. Сначала ждем инициализации клиента
            await InitSupabase();

            // 2. Затем работаем с пользователем
            await ListenUser();

            // 3. Только потом создаем список, когда клиент и пользователь готовы
            requestsList = new Pages.RequestsList(_supabase, currentProfile, (r => 
            {
                var Page = new EditRequest(r, _supabase, (u =>
                {
                    NavigationHost.Navigate(requestsList);
                    if (u) requestsList.LoadData();
                }));
                NavigationHost.Navigate(Page);
            }));
            NavigationHost.Navigate(requestsList);
        }


        private async Task ListenUser()
        {
            // Получаем текущую сессию
            var session = _supabase.Auth.CurrentSession;

            if (session == null)
            {
                var authWindow = new AuthWindow(_supabase);
                authWindow.ShowDialog();

                // ВАЖНО: берем обновленную сессию из окна после его закрытия
                session = authWindow.Session;
            }

            // Если пользователь так и не вошел (закрыл окно), не продолжаем
            if (session?.User == null) return;

            try
            {
                // 4. ИСПРАВЛЕНИЕ: Загружаем профиль из таблицы Profiles, а не RepairRequest
                // И используем поле класса currentProfile (без var), чтобы данные сохранились
                currentProfile = await _supabase
                    .From<Profile>() // Убедитесь, что сущность Profile сопоставлена с таблицей в БД
                    .Filter("id", Postgrest.Constants.Operator.Equals, session.User.Id)
                    .Single();
               

                if (currentProfile == null)
                {
                    throw new Exception("qwrtyuiop");
                }

                NameText.Text = currentProfile.Name;
                RoleText.Text = currentProfile.Role;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}");
            }
        }

        private async Task InitSupabase()
        {
            string url = "https://hlczwmextdxrpgrhbamx.supabase.co";
            string key = "sb_publishable_TWcgpnJeYc_uHwtqywm5MA_3fusT1Lq";
            _supabase = new Supabase.Client(url, key);
            await _supabase.InitializeAsync();
        }

        private void BtnAddRequest_Click(object sender, RoutedEventArgs e)
        {
            // Снимаем выделение с фильтров, так как мы уходим на другую страницу
            OpenRequests.IsChecked = false;
            AcceptedRequests.IsChecked = false;
            ArchivedRequests.IsChecked = false;

            var addPage = new Pages.AddRequest(_supabase, (updated =>
            {
                requestsList.LoadData();
                NavigationHost.Navigate(requestsList);
                // Возвращаем выделение на "Открытые", когда вернулись
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
    }
}