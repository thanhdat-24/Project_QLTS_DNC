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
                
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)  
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

               
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
                
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc cấu hình Supabase: {ex.Message}");
                throw; 
            }
        }
    }
}
