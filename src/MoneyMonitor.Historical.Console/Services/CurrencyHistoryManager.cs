using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Trader.Console.Infrastructure;
using Newtonsoft.Json;

namespace MoneyMonitor.Historical.Console.Services
{
    public class CurrencyHistoryManager
    {
        private readonly CoinbaseProExchangeClient _client;

        private readonly Output _output;

        public CurrencyHistoryManager(ILogger logger)
        {
            var settings = Settings.Instance;

            _client = new CoinbaseProExchangeClient(settings.CoinbaseProCredentials.ApiKey,
                                                    settings.CoinbaseProCredentials.ApiSecret,
                                                    settings.CoinbaseProCredentials.Passphrase,
                                                    settings.FiatCurrency,
                                                    null, 
                                                    null, 
                                                    logger);

            _output = new Output("history-data.csv");

            _output.Write("Boot", ConsoleColor.White);
        }

        public async Task ExecuteAsync(string currency)
        {
            string granularity = "900"; // 15 mins
            DateTime start = DateTime.Parse("2021-01-01");

            while (true)
            {
                DateTime end = start + TimeSpan.FromDays(1);
                _output.Write($"{start} => {end}", ConsoleColor.Green);                

                var result = await _client.GetHistory(currency, start, end, granularity);

                // Dump to cache
                DirectoryInfo di = new DirectoryInfo(@$"C:\_Git\_TimeCache\{currency}\{granularity}");
                if (!di.Exists) di.Create();
                foreach (var candle in result)
                {
                    var filename = Path.Combine(di.FullName, candle.time.ToString(CultureInfo.InvariantCulture) + ".dat");
                    using StreamWriter rw = new(filename);
                    string json = JsonConvert.SerializeObject(candle, Formatting.None);
                    rw.WriteLine(json);
                }

                start = end;
                if (start > DateTime.Now)
                {
                    break;
                }
            }
        }
    }
}