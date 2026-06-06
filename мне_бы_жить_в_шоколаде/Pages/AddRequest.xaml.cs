using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace мне_бы_жить_в_шоколаде.Pages
{
    
    public partial class AddRequest : Page
    {

        private readonly Action<bool> _onClose;

        public AddRequest(Action<bool> onClose)
        {
            InitializeComponent();
            _onClose = onClose;
            LoadEquipment();
        }

        private async void LoadEquipment()
        {
            var client = await Globals.GetClient();
            var response = await client.From<Equipment>().Get();
            EquipmentCombo.ItemsSource = response.Models;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (EquipmentCombo.SelectedValue == null || string.IsNullOrWhiteSpace(DescriptionText.Text))
            {
                MessageBox.Show("Пожалуйста, выберите оборудование и опишите проблему.");
                return;
            }

            try
            {
                var client = await Globals.GetClient();

                var newRequest = new RepairRequest
                {
                    EquipmentId = (Guid)EquipmentCombo.SelectedValue,
                    RequesterId = Guid.Parse(client.Auth.CurrentUser.Id), 
                    Description = DescriptionText.Text,
                    Priority = (PriorityCombo.SelectedItem as FrameworkElement)?.Tag.ToString(),
                    Status = "new",
                    Deadline = DeadlinePicker.SelectedDate?.ToUniversalTime(),
                    CreatedAt = DateTime.Now
                };

                
                await client.From<RepairRequest>().Insert(newRequest);

                MessageBox.Show("Заявка успешно создана!");
                _onClose(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _onClose(false);
        }
    }
}

