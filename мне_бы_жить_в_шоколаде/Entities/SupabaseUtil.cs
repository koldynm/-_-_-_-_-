using Supabase.Gotrue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace мне_бы_жить_в_шоколаде.Entities
{
    public static class SupabaseUtil
    {
        public static async Task<Supabase.Client> InitSupabase()
        {
            string url = "https://hlczwmextdxrpgrhbamx.supabase.co";
            string key = "sb_publishable_TWcgpnJeYc_uHwtqywm5MA_3fusT1Lq";
            var _supabase = new Supabase.Client(url, key);
            await _supabase.InitializeAsync();
            return _supabase;
        }
    }
}
