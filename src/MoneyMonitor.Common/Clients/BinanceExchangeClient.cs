using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Models.BinanceApiResponses;

namespace MoneyMonitor.Common.Clients
{
    public class BinanceExchangeClient : ICryptoExchangeClient
    {
        private readonly HttpClient _client;

        private readonly string _secretKey;

        public BinanceExchangeClient(string apiKey, string secretKey)
        {
            _client = new HttpClient
                      {
                          BaseAddress = new Uri("https://api.binance.com")
                      };

            _secretKey = secretKey;

            _client.DefaultRequestHeaders.Add("X-MBX-APIKEY", apiKey);
        }

        public async Task<List<ExchangeBalance>> GetBalances()
        {
            var balances = await GetCoinBalances();

            return null;
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
                if (account.Free > 0)
                {
                    balances.Add(new ExchangeBalance
                                 {
                                     Amount = account.Free,
                                     Currency = account.Asset
                                 });
                }
            }

            return balances;
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