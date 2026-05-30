using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using мне_бы_жить_в_шоколаде.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace мне_бы_жить_в_шоколаде.Pages
{
    public partial class RequestsList : Page
    {
        private Supabase.Client _supabase;
        private Profile _profile;
        private Action<RepairRequest> _editRequest;
        private Action<RepairRequest> _controlRequest;

        private string filterStatus = "new";

        public async void SetFilterStatus(string filterStatus)
        {
            this.filterStatus = filterStatus;
            LoadData();
            UpdateUI();
        }

        private void UpdateUI()
        {
            TakeRequestButton.Visibility = (_profile.Role == "technician" && filterStatus == "new") ? Visibility.Visible : Visibility.Collapsed;
            ControlRequestButton.Visibility = (filterStatus == "in_progress") ? Visibility.Visible : Visibility.Collapsed;
            EditRequestButton.Visibility = (_profile.Role == "admin") ? Visibility.Visible : Visibility.Collapsed;
            DeleteRequestButton.Visibility = (_profile.Role == "admin") ? Visibility.Visible : Visibility.Collapsed;


        }


        public RequestsList(Supabase.Client supabase, Profile profile, Action<RepairRequest> editRequest, Action<RepairRequest> controlRequest) 
        {
            InitializeComponent();
            _supabase = supabase;
            _profile = profile;
            _editRequest = editRequest;
            _controlRequest = controlRequest;
            UpdateUI();

            LoadData();
        }

        public async void LoadData()
        {
            try
            {
                var response = await _supabase
                    .From<RepairRequest>()
                    .Filter("status", Postgrest.Constants.Operator.Equals, filterStatus)
                    .Get();
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
                await _supabase.From<RepairRequest>()
                    .Where(r => r.Id == request.Id)
                    .Set(r => r.TechnicianId, _profile.Id)
                    .Set(r => r.Status, "in_progress")
                    .Update();
                LoadData();
            }
            else
            {
                MessageBox.Show("Выберите задачу");
            }
        }
        
        private async void EditRequestButton_Click(object sender, RoutedEventArgs e)
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
                await _supabase.From<RepairRequest>()
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
    }
}
