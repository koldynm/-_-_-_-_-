using Supabase.Gotrue;
using Supabase;
using мне_бы_жить_в_шоколаде.Entities;
using System.Windows;

namespace мне_бы_жить_в_шоколаде
{
    public static class Globals
    {
        private static bool IsProfileLoaded { get; set; } = false;
        private static Supabase.Client? Client {  get; set; }
        private static Profile? Profile { get; set; }

        public static AdminClient? AdminClient { get; set; }
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
    }
}
