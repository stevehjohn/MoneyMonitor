using System;
using System.IO;
using System.Text;
using System.Text.Json;
using MoneyMonitor.Common.Infrastructure;

namespace MoneyMonitor.Trader.Console.Infrastructure.Settings
{
    public class ConsoleSettings
    {
        public CoinbaseProCredentials CoinbaseProCredentials { get; set; }

        public string FiatCurrency { get; set; }
        
        public TimeSpan PollInterval { get; set; }

        public TradeParameters[] TradeParameters { get; set; }

        public static ConsoleSettings Instance => Lazy.Value;

        private static readonly Lazy<ConsoleSettings> Lazy = new(GetAppSettings);

        public void Save()
        {
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                                                      {
                                                          Converters = { new TimeSpanConverter() }
                                                      });

            File.WriteAllText("consoleSettings.json", json, Encoding.UTF8);
        } 

        private static ConsoleSettings GetAppSettings()
        {
            var json = File.ReadAllText("consoleSettings.json", Encoding.UTF8);

            var settings = JsonSerializer.Deserialize<ConsoleSettings>(json, new JsonSerializerOptions
                                                                         {
                                                                             Converters = { new TimeSpanConverter() }
                                                                         });

            return settings;
        }
    }
}