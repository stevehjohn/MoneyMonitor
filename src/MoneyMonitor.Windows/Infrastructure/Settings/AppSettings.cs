using System;
using System.IO;
using System.Text;
using System.Text.Json;
using MoneyMonitor.Common.Infrastructure;

namespace MoneyMonitor.Windows.Infrastructure.Settings
{
    public class AppSettings
    {
        public bool AlwaysOnTop { get; set; }

        public int BalanceHigh { get; set; }

        public int BalanceLow { get; set; }

        public CoinbaseCredentials CoinbaseCredentials { get; set; }

        public string FiatCurrency { get; set; }

        public string FiatCurrencySymbol { get; set; }

        public TimeSpan PollInterval { get; set; }

        public static AppSettings Instance => Lazy.Value;

        private static readonly Lazy<AppSettings> Lazy = new(GetAppSettings);

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);

            File.WriteAllText("appSettings.json", json, Encoding.UTF8);
        } 

        private static AppSettings GetAppSettings()
        {
            var json = File.ReadAllText("appSettings.json", Encoding.UTF8);

            var settings = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                                                                         {
                                                                             Converters = { new TimeSpanConverter() }
                                                                         });

            return settings;
        }
    }
}