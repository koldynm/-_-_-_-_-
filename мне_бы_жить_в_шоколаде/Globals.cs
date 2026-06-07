using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using System.Configuration;
using System.Windows;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде
{
    public static class Globals
    {
        private static readonly SemaphoreSlim _lock = new(1, 1);

        private static bool _isProfileLoaded;
        private static Supabase.Client? _client;
        private static Profile? _profile;
        private static IGotrueAdminClient<User>? _adminAuth;

        public static Session? Session => _client?.Auth?.CurrentSession;
        public static Guid? CurrentUserId
        {
            get {
                var idRaw = Session?.User?.Id;

                if (idRaw is null) return null;

                bool parsed = Guid.TryParse(idRaw, out Guid id);
                return parsed ? id : null;
            }
        }

        // ─── Client ────────────────────────────────────────────────────────────

        public static async Task<Supabase.Client> GetClient()
        {
            if (_client is not null) return _client;

            await _lock.WaitAsync();
            try
            {
                _client ??= await SupabaseUtil.InitSupabase();
            }
            finally
            {
                _lock.Release();
            }

            return _client;
        }

        // ─── Profile ───────────────────────────────────────────────────────────

        public static async Task<Profile?> GetProfile(bool refresh = false)
        {
            if (Session is null) return null;
            if (_isProfileLoaded && !refresh) return _profile;

            try
            {
                var client = await GetClient();
                var userId = Session.User?.Id
                    ?? throw new InvalidOperationException("ID пользователя отсутствует в сессии.");

                _profile = await client
                    .From<Profile>()
                    .Filter("id", Postgrest.Constants.Operator.Equals, userId)
                    .Single();

                _isProfileLoaded = true;
            }
            catch (Exception ex)
            {
                _isProfileLoaded = false;
                MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}",
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return _profile;
        }

        public static async Task<Profile> RequireProfile(bool refresh = false)
            => await GetProfile(refresh)
               ?? throw new InvalidOperationException("Профиль не загружен или пользователь не авторизован.");

        // ─── Admin ─────────────────────────────────────────────────────────────

        public static async Task<IGotrueAdminClient<User>> GetAdminAuth()
        {
            var client = await GetClient();
            var profile = await RequireProfile();

            if (!AppRoles.IsAdmin(profile.Role))
                throw new UnauthorizedAccessException("Доступ запрещён: требуется роль администратора.");

            if (_adminAuth is not null) return _adminAuth;

            var serviceKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhsY3p3bWV4dGR4cnBncmhiYW14Iiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc3MjY3MzQxOCwiZXhwIjoyMDg4MjQ5NDE4fQ.qbfctNn4NQ4dsKl9M6uW_l-L-qOAZ6CDgAqSYsAlcmg";

            _adminAuth = client.AdminAuth(serviceKey);
            return _adminAuth;
        }

        // ─── Сброс состояния (при выходе из аккаунта) ──────────────────────────

        public static void Reset()
        {
            _profile = null;
            _isProfileLoaded = false;
            _adminAuth = null;
        }
    }
}