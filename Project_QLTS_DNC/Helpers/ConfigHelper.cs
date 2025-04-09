using Microsoft.Extensions.Configuration;
using System.IO;

namespace Project_QLTS_DNC.Helpers
{
    public static class ConfigHelper
    {
        public static SupabaseSettings GetSupabaseSettings()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var settings = new SupabaseSettings();
            config.GetSection("Supabase").Bind(settings);
            return settings;
        }
    }
}
