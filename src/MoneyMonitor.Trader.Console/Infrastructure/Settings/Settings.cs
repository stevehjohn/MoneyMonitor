using System;
using System.IO;
using System.Text;
using System.Text.Json;
using MoneyMonitor.Common.Infrastructure;

namespace MoneyMonitor.Trader.Console.Infrastructure.Settings
{
    public class Settings
    {
        public CoinbaseProCredentials CoinbaseProCredentials { get; set; }

        public string FiatCurrency { get; set; }
        
        public TimeSpan PollInterval { get; set; }

        public TradeParameters[] TradeParameters { get; set; }

        public static Settings Instance => Lazy.Value;

        private static readonly Lazy<Settings> Lazy = new(GetAppSettings);

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                                                      {
                                                          Converters = { new TimeSpanConverter() }
                                                      });

            File.WriteAllText("consoleSettings.json", json, Encoding.UTF8);
        } 

        private static Settings GetAppSettings()
        {
            var json = File.ReadAllText("consoleSettings.json", Encoding.UTF8);

            var settings = JsonSerializer.Deserialize<Settings>(json, new JsonSerializerOptions
                                                                         {
                                                                             Converters = { new TimeSpanConverter() }
                                                                         });

            return settings;
        }
    }
}