using Project_QLTS_DNC.Helpers;
using Project_QLTS_DNC.Models;
using Supabase;
using System;
using System.Threading.Tasks;

namespace Project_QLTS_DNC.Services
{
    public class SupabaseService
    {
        private static Supabase.Client _client;

        public static async Task<Supabase.Client> GetClientAsync()
        {
            if (_client != null) return _client;

            var settings = ConfigHelper.GetSupabaseSettings(); // Lấy cấu hình Supabase
            _client = new Supabase.Client(settings.Url, settings.ApiKey); // Khởi tạo Supabase.Client

            // Khởi tạo Supabase client (PostgrestClient và AuthClient được cấu hình tự động)
            await _client.InitializeAsync();
            return _client;
        }






    }
}
