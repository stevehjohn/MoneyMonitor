using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace MoneyMonitor.Windows.Infrastructure.Settings
{
    public class AppSettings
    {
        public CoinbaseCredentials CoinbaseCredentials { get; set; }

        public static AppSettings Instance => Lazy.Value;

        private static readonly Lazy<AppSettings> Lazy = new(GetAppSettings);

        private static AppSettings GetAppSettings()
        {
            var json = File.ReadAllText("appSettings.json", Encoding.UTF8);

            var settings = JsonSerializer.Deserialize<AppSettings>(json);

            return settings;
        }
    }
}