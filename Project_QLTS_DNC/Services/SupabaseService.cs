using Project_QLTS_DNC.Helpers;
using Supabase;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class SupabaseService
    {
        private static Client _client;

        public static async Task<Client> GetClientAsync()
        {
            if (_client != null) return _client;

            var settings = ConfigHelper.GetSupabaseSettings();
            _client = new Client(settings.Url, settings.ApiKey, new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true
            });

            await _client.InitializeAsync();
            return _client;
        }
    }
}
