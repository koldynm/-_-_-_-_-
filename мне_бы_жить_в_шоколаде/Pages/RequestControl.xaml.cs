using System.Windows;
using System.Windows.Controls;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде.Pages;

public partial class RequestControl : Page
    {
        private Supabase.Client _supabase;
        private RepairRequest _currentRequest;
        private Profile _profile;
        private Action _close;

        public RequestControl(Supabase.Client supabase, RepairRequest request, Profile profile, Action close)
        {
            InitializeComponent();
            _supabase = supabase;
            _currentRequest = request;
            _profile = profile;
            _close = close;
            
            BtnComplete.Visibility = (_profile.Role == "technician" ) ? Visibility.Visible : Visibility.Collapsed;
            BtnRefuse.Visibility = (_profile.Role == "technician" ) ? Visibility.Visible : Visibility.Collapsed;

            LoadRequestDetails();
            LoadMessages();
        }

        private void LoadRequestDetails()
        {
            TxtTitle.Text = "Заявка #" + _currentRequest.Id.ToString().Substring(0, 5);
            TxtCreatedAt.Text = _currentRequest.CreatedAt.ToString("f");
            TxtDescription.Text = _currentRequest.Description;
            
            TxtDeadline.Text = _currentRequest.Deadline.HasValue 
                ? _currentRequest.Deadline.Value.ToString("f") 
                : "Не указан";

            if (_currentRequest.CompletedAt.HasValue)
            {
                BtnComplete.Visibility = Visibility.Collapsed;
                TxtCompletedInfo.Visibility = Visibility.Visible;
                TxtCompletedInfo.Text = $"Завершена: {_currentRequest.CompletedAt.Value:f}";
            }
        }

        private async void LoadMessages()
        {
            try
            {
                var response = await _supabase.From<ChatMessage>()
                    .Filter("request_id", Postgrest.Constants.Operator.Equals, _currentRequest.Id.ToString())
                    .Order("created_at",  Postgrest.Constants.Ordering.Ascending)
                    .Get();
                
                List<UiChatMessage> messages = [  ];
                foreach (var msg in response.Models)
                {
                    var uiMsg = new UiChatMessage
                    {
                        Id = msg.Id,
                        RequestId = msg.RequestId,
                        SenderId = msg.SenderId,
                        MessageText = msg.MessageText,
                        CreatedAt = msg.CreatedAt,
                        IsOwnMessage = msg.SenderId == _profile.Id
                    };
                    messages.Add(uiMsg);
                }
                
                MessagesList.ItemsSource = null;
                MessagesList.ItemsSource = messages;
                ChatScroller.ScrollToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки чата: " + ex.Message);
            }
        }

        private async void BtnSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNewMessage.Text)) return;

            var msg = new ChatMessage
            {
                RequestId = _currentRequest.Id,
                SenderId = _profile.Id,
                MessageText = TxtNewMessage.Text.Trim(),
                CreatedAt = DateTime.Now
            };

            await _supabase.From<ChatMessage>().Insert(msg);
            TxtNewMessage.Clear();
            LoadMessages();
        }

        private async void BtnComplete_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.UtcNow;
            var update = await _supabase.From<RepairRequest>()
                .Where(x => x.Id == _currentRequest.Id)
                .Set(x => x.CompletedAt, now)
                .Set(x => x.Status, "closed")
                .Update();
            _close();

            _currentRequest.CompletedAt = now;
            LoadRequestDetails();
            MessageBox.Show("Заявка успешно завершена!");
            
        }

        private async void BtnRefuse_OnClick(object sender, RoutedEventArgs e)
        {
            await _supabase.From<RepairRequest>()
                .Where(r => r.Id == _currentRequest.Id)
                .Set(r => r.TechnicianId, null)
                .Set(r => r.Status, "new")
                .Update();
            _close();
        }
    }