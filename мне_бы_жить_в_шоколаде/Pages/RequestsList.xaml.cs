using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде.Pages
{
    public partial class RequestsList : Page
    {
        private readonly Action<RepairRequest> _editRequest;
        private readonly Action<RepairRequest> _controlRequest;
        private readonly Action<RepairRequest> _requestInfo;

        private string filterStatus = "new";

        public void SetFilterStatus(string filterStatus)
        {
            RequestsGrid.ItemsSource = null;
            this.filterStatus = filterStatus;
            LoadData();
            UpdateUI();
        }

        private async void UpdateUI()
        {
            var profile = await Globals.RequireProfile();

            TakeRequestButton.Visibility = (AppRoles.IsTechnician(profile.Role) && filterStatus == "new") ? Visibility.Visible : Visibility.Collapsed;
            ControlRequestButton.Visibility = (filterStatus == "in_progress") ? Visibility.Visible : Visibility.Collapsed;
            EditRequestButton.Visibility = AppRoles.IsAdmin(profile.Role) ? Visibility.Visible : Visibility.Collapsed;
            DeleteRequestButton.Visibility = AppRoles.IsAdmin(profile.Role) ? Visibility.Visible : Visibility.Collapsed;
            RequestInfoButton.Visibility = filterStatus == "new" || filterStatus == "closed" ? Visibility.Visible : Visibility.Collapsed;
        }


        public RequestsList(
            Action<RepairRequest> editRequest, 
            Action<RepairRequest> controlRequest,
            Action<RepairRequest> requestInfo
        ) 
        {
            InitializeComponent();
            _editRequest = editRequest;
            _controlRequest = controlRequest;
            _requestInfo = requestInfo;
            UpdateUI();

            LoadData();
        }

        public async void LoadData()
        {
            try
            {

                var client = await Globals.GetClient();
                var profile = await Globals.RequireProfile();

                System.Diagnostics.Debug.WriteLine($"Загрузка запросов ({filterStatus}, {profile.Id})");

                var response = await client
                    .From<RepairRequest>()
                    .Filter("status", Postgrest.Constants.Operator.Equals, filterStatus)
                    .Get();

                System.Diagnostics.Debug.WriteLine($"Запросы загружены ({response.Models.Count})");

                var requests = response.Models;

                RequestsGrid.ItemsSource = requests;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
            }
        }

        private void RequestsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private async void TakeRequest_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsGrid.SelectedItem is RepairRequest request)
            {
                try
                {
                    var client = await Globals.GetClient();
                    var profile = await Globals.RequireProfile();

                    await client.From<RepairRequest>()
                        .Where(r => r.Id == request.Id)
                        .Set(r => r.TechnicianId, profile.Id)
                        .Set(r => r.Status, "in_progress")
                        .Update();
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Выберите задачу");
            }
        }
        
        private void EditRequestButton_Click(object sender, RoutedEventArgs e)
        {

            if (RequestsGrid.SelectedItem is RepairRequest request)
            {
                _editRequest(request);
            }
            else
            {
                MessageBox.Show("Выберите задачу");
            }
        }

        private async void DeleteRequestButton_Click(object sender, RoutedEventArgs e)
        {

            if (RequestsGrid.SelectedItem is RepairRequest request)
            {
                var client = await Globals.GetClient();
                await client.From<RepairRequest>()
                    .Where(r => r.Id == request.Id)
                    .Delete();
                LoadData();
            }
            else
            {
                MessageBox.Show("Выберите задачу");
            }

        }

        private void ControlRequestButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RequestsGrid.SelectedItem is RepairRequest request)
            {
                _controlRequest(request);
            }
            else
            {
                MessageBox.Show("Выберите задачу");
            }
        }

        private void RequestInfoButton_Click(object sender, RoutedEventArgs e)
        {
            if (RequestsGrid.SelectedItem is RepairRequest request)
            {
                _requestInfo(request);
            }
            else
            {
                MessageBox.Show("Выберите задачу");
            }
        } 
    }
}
