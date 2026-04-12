using Supabase.Gotrue;
using System;
using System.Windows;

namespace мне_бы_жить_в_шоколаде
{
    public partial class AuthWindow : Window
    {
        Supabase.Client _supabase;
        public Session Session;
        public AuthWindow(Supabase.Client supabase)
        {
            InitializeComponent();
            _supabase = supabase;
        }

        // Добавили async, чтобы await работал
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
                // Убедись, что переменная supabase инициализирована где-то в твоем проекте
                var session = await _supabase.Auth.SignIn(email, password);
                Session = session;
                this.DialogResult = true;
                // Переход на другое окно, например:
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
    }
}