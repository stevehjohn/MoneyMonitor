using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Models.BinanceApiResponses;

namespace MoneyMonitor.Common.Clients
{
    public class BinanceExchangeClient : ICryptoExchangeClient
    {
        private readonly HttpClient _client;

        private readonly string _secretKey;

        private readonly string _fiatCurrency;

        public BinanceExchangeClient(string apiKey, string secretKey, string fiatCurrency)
        {
            _client = new HttpClient
                      {
                          BaseAddress = new Uri("https://api.binance.com")
                      };

            _secretKey = secretKey;

            _fiatCurrency = fiatCurrency;

            _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);
        }

        public async Task<List<ExchangeBalance>> GetBalances()
        {
            var balances = await GetCoinBalances();

            var result = new List<ExchangeBalance>();

            if (balances.Count == 0)
            {
                return result;
            }

            var exchangeRates = await GetExchangeRates(balances.Select(r => r.Currency).ToList());

            var now = DateTime.UtcNow;

            foreach (var coinBalance in balances)
            {
                if (! exchangeRates.ContainsKey(coinBalance.Currency))
                {
                    // Not sure why some currencies don't convert...
                    continue;
                }

                var rate = exchangeRates[coinBalance.Currency];

                result.Add(new ExchangeBalance
                           {
                               Amount = coinBalance.Amount,
                               Currency = coinBalance.Currency,
                               ExchangeRate = 1 / rate,
                               TimeUtc = now,
                               Value = (int) (coinBalance.Amount * rate * 100)
                           });
            }

            return result;
        }
        
        private async Task<List<ExchangeBalance>> GetCoinBalances()
        {
            var balances = new List<ExchangeBalance>();

            var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v3/account?{BuildQueryString()}");

            var response = await _client.SendAsync(message);

            var stringData = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<Account>(stringData);
                
            // ReSharper disable once PossibleNullReferenceException
            foreach (var account in data.Balances)
            {
                var balance = decimal.Parse(account.Free);

                if (balance > 0)
                {
                    balances.Add(new ExchangeBalance
                                 {
                                     Amount = balance,
                                     Currency = account.Asset
                                 });
                }
            }

            return balances;
        }

        private async Task<Dictionary<string, decimal>> GetExchangeRates(List<string> coins)
        {
            var rates = new Dictionary<string, decimal>();

            foreach (var coin in coins)
            {
                var message = new HttpRequestMessage(HttpMethod.Get, $"/api/v3/ticker/price?symbol={coin}{_fiatCurrency}");

                var response = await _client.SendAsync(message);

                if (! response.IsSuccessStatusCode)
                {
                    // Not sure why some currencies don't convert...
                    continue;
                }

                var data = JsonSerializer.Deserialize<Ticker>(await response.Content.ReadAsStringAsync());

                // ReSharper disable once PossibleNullReferenceException
                rates.Add(coin, decimal.Parse(data.Price));
            }

            return rates;
        }

        private string BuildQueryString()
        {
            var timestamp = $"{(long) DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalMilliseconds}";
            
            var query = $"timestamp={timestamp}";

            var bytes = Encoding.ASCII.GetBytes(query);

            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(_secretKey));

            var hash = hmacsha256.ComputeHash(bytes);

            return $"{query}&signature={BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant()}";
        }
    }
}