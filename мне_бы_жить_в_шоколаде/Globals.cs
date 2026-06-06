using Supabase;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;
using System.Windows;
using мне_бы_жить_в_шоколаде.Entities;

namespace мне_бы_жить_в_шоколаде
{
    public static class Globals
    {
        private static bool IsProfileLoaded { get; set; } = false;
        private static Supabase.Client? Client {  get; set; }
        private static Profile? Profile { get; set; }
        private static IGotrueAdminClient<User>? AdminAuth { get; set; }
        public static Session? Session => Client?.Auth?.CurrentSession;

        public static async Task<Supabase.Client> GetClient()
        {
            Client ??= await SupabaseUtil.InitSupabase();

            return Client;
        }
        public static async Task<Profile?> GetProfile(bool refresh = false)
        {
            if (Session is not null && (!IsProfileLoaded || refresh))
            {
                try
                {
                    var client = await GetClient();
                    Profile = await client
                        .From<Profile>()
                        .Filter("id", Postgrest.Constants.Operator.Equals, Session.User.Id)
                        .Single();
                    IsProfileLoaded = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}");
                }
            }
            return Profile;
        }
        public static async Task<Profile> RequireProfile(bool refresh = false)
        {
            var profile = await GetProfile(refresh);
            if (profile is null) throw new MethodAccessException("Нет профиля");
            return profile;
        }

        public static async Task<IGotrueAdminClient<User>> GetAdminAuth()
        {
            var client = await GetClient();
            var profile = await RequireProfile();

            if (!AppRoles.IsAdmin(profile.Role)) throw new MethodAccessException("Не админ");

            AdminAuth ??= client.AdminAuth("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhsY3p3bWV4dGR4cnBncmhiYW14Iiwicm9sZSI6InNlcnZpY2Vfcm9sZSIsImlhdCI6MTc3MjY3MzQxOCwiZXhwIjoyMDg4MjQ5NDE4fQ.qbfctNn4NQ4dsKl9M6uW_l-L-qOAZ6CDgAqSYsAlcmg");

            return AdminAuth;
        }
    }
}
