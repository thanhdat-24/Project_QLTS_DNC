using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Project_QLTS_DNC.Helpers
{
    public static class ConfigHelper
    {
        public static SupabaseSettings GetSupabaseSettings()
        {
            try
            {
                // Xây dựng Configuration từ file appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)  // Đảm bảo lấy đúng thư mục gốc của ứng dụng
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Đọc cấu hình Supabase từ file
                var settings = new SupabaseSettings();
                config.GetSection("Supabase").Bind(settings);

                if (string.IsNullOrEmpty(settings.Url) || string.IsNullOrEmpty(settings.ApiKey))
                {
                    throw new InvalidOperationException("Supabase URL or API Key is missing in configuration.");
                }

                return settings;
            }
            catch (Exception ex)
            {
                // Ghi log lỗi khi không thể tải cấu hình
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc cấu hình Supabase: {ex.Message}");
                throw; // Ném lại exception để lớp gọi phía trên xử lý
            }
        }
    }
}
