using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Trader.Console.Infrastructure;
using Newtonsoft.Json;
using static MoneyMonitor.Common.Clients.CoinbaseProExchangeClient;

namespace MoneyMonitor.Historical.Console.Services
{
    public sealed class CurrencyHistoryManager
    {
        const string granularity = "900"; // 15 mins

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
            DirectoryInfo _cache = new DirectoryInfo(@$"C:\_Git\_TimeCache\{currency}\{granularity}");
            if (!_cache.Exists) _cache.Create();

            FileInfo[] files = _cache.GetFiles("*.dat");

            using StreamWriter rw = new StreamWriter(@"C:\_Git\_TimeCache\report.csv");
            foreach (FileInfo file in files)
            {
                string json = File.ReadAllText(file.FullName);
                HistoryResponse candle = JsonConvert.DeserializeObject<HistoryResponse>(json);
                var date = DateTime.UnixEpoch + TimeSpan.FromSeconds(candle.time);
                rw.WriteLine($"{date},{candle.low},{candle.high},{candle.open},{candle.close}");
            }
        }

        public async Task BuildCacheAsync(string currency)
        {
            DirectoryInfo _cache = new DirectoryInfo(@$"C:\_Git\_TimeCache\{currency}\{granularity}");
            if (!_cache.Exists) _cache.Create();

            DateTime start = DateTime.Parse("2021-01-01");

            while (true)
            {
                DateTime end = start + TimeSpan.FromDays(1);
                _output.Write($"{start} => {end}", ConsoleColor.Green);                

                var result = await _client.GetHistory(currency, start, end, granularity);

                // Dump to cache
                foreach (var candle in result)
                {
                    var filename = Path.Combine(_cache.FullName, candle.time.ToString(CultureInfo.InvariantCulture) + ".dat");
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