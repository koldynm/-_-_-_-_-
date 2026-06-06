using Supabase.Gotrue;
using System;
using System.Windows;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде
{
    public partial class AuthWindow : Window
    {
        public AuthWindow()
        {
            InitializeComponent();
            init();
        }
        private async void init()
        {
            if (Globals.Session?.User != null) 
            {
                ShowRequests();
            }

        }
        private void ShowRequests()
        {
            var window = new MainWindow();
            window.Show();
            Close();
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var email = emailTextBox.Text.Trim();
            var password = passwordPasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            try
            {
                var client = await Globals.GetClient();
                await client.Auth.SignIn(email, password);
                ShowRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: Неправильный логин или пароль!");
            }
        }
    }
}