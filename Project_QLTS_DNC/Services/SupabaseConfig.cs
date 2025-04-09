using Supabase;
using System.Threading.Tasks;

namespace Project_QLTS_DNC
{
    public static class SupabaseConfig
    {
        public static Supabase.Client SupabaseClient { get; private set; }

        private static readonly string SupabaseUrl = "https://hoybfwnugefnpctgghha.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImhveWJmd251Z2VmbnBjdGdnaGhhIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQxMDQ4OTEsImV4cCI6MjA1OTY4MDg5MX0.KxNfiOUFXHGgqZf3b3xOk6BR4sllMZG_-W-y_OPUwCI";

        private static bool isInitialized = false;

        public static async Task InitializeAsync()
        {
            if (isInitialized) return;

            var options = new SupabaseOptions
            {
                AutoConnectRealtime = true
            };

            SupabaseClient = new Client(SupabaseUrl, SupabaseKey, options);
            await SupabaseClient.InitializeAsync();
            isInitialized = true;
        }
    }
}
