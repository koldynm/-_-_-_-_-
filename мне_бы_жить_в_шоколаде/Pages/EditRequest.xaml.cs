using System;
using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Entities;
using Supabase;

namespace мне_бы_жить_в_шоколаде.Pages
{
    public partial class EditRequest : Page
    {
        private readonly Supabase.Client _supabase;
        private readonly Action<bool> _onClose;

        public RepairRequest Request { get; set; }

        public EditRequest(RepairRequest request, Supabase.Client supabase, Action<bool> onClose)
        {
            InitializeComponent();
            _supabase = supabase;
            _onClose = onClose;
            Request = request;

            this.DataContext = this;
            LoadEquipment();

            SetEnums();
        }

        private async void LoadEquipment()
        {
            var response = await _supabase.From<Equipment>().Get();
            EquipmentCombo.ItemsSource = response.Models;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Request.UpdatedAt = DateTime.Now;
                
                if (Request.Status == "Completed")
                {
                    Request.ClosedAt = DateTime.Now;
                }

                await _supabase.From<RepairRequest>()
                               .Where(x => x.Id == Request.Id)
                               .Update(Request);

                MessageBox.Show("Заявка успешно обновлена!");
                _onClose?.Invoke(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _onClose?.Invoke(false);
        }

        private void SetEnums()
        {
            RequestStatusCB.ItemsSource = EnumValue.RequestStatuses;
            RequestPriorityCB.ItemsSource = EnumValue.RequestPriorities;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}