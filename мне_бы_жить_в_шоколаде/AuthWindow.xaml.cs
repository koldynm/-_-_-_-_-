using Supabase.Gotrue;
using System;
using System.Windows;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде
{
    public partial class AuthWindow : Window
    {
        Supabase.Client _supabase;
        Session? session;
        public AuthWindow(Supabase.Client? supabase)
        {
            InitializeComponent();
            init(supabase);

        }
        public AuthWindow()
        {
            InitializeComponent();
            init(null);

        }
        private async void init(Supabase.Client? supabase)
        {
            _supabase = supabase ?? await SupabaseUtil.InitSupabase();
            session = _supabase.Auth.CurrentSession;
            if (session != null && session?.User != null) 
            {
                ShowRequests();
            }

        }
        private void ShowRequests()
        {
            var window = new MainWindow(_supabase, session);
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
                session = await _supabase.Auth.SignIn(email, password);
                ShowRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: Неправильный логин или пароль!");
            }
        }
    }
}