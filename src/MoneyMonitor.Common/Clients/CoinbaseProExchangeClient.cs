using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Models.CoinbaseProApiResponses;

namespace MoneyMonitor.Common.Clients
{
    public class CoinbaseProExchangeClient : ICryptoExchangeClient
    {
        private readonly HttpClient _client;

        private readonly string _apiSecret;
        private readonly string _fiatCurrency;

        public CoinbaseProExchangeClient(string apiKey, string apiSecret, string passphrase, string fiatCurrency)
        {
            _client = new HttpClient
                      {
                          BaseAddress = new Uri("https://api.pro.coinbase.com")
                      };

            _client.DefaultRequestHeaders.Add("User-Agent", "CoinbaseProApiClient");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("CB-ACCESS-KEY", apiKey);
            _client.DefaultRequestHeaders.Add("CB-ACCESS-PASSPHRASE", passphrase);

            _apiSecret = apiSecret;
            _fiatCurrency = fiatCurrency;
        }

        public async Task<List<ExchangeBalance>> GetBalances()
        {
            var balances = await GetCoinBalances();

            var result = new List<ExchangeBalance>();

            if (balances.Count == 0)
            {
                return result;
            }

            var exchangeRates = await GetExchangeRates(balances.Select(b => b.Currency).ToList());

            var now = DateTime.UtcNow;

            foreach (var coinBalance in balances)
            {
                // TODO: What to do if exchange rate not found?
                if (! exchangeRates.ContainsKey(coinBalance.Currency))
                {
                    continue;
                }

                var rate = exchangeRates[coinBalance.Currency];

                result.Add(new ExchangeBalance
                           {
                               Amount = coinBalance.Amount,
                               Currency = coinBalance.Currency,
                               ExchangeRate = rate,
                               TimeUtc = now,
                               Value = (int) (coinBalance.Amount / rate * 100)
                           });
            }

            return result;
        }
 
        private async Task<List<ExchangeBalance>> GetCoinBalances()
        {
            var balances = new List<ExchangeBalance>();

            var message = new HttpRequestMessage(HttpMethod.Get, "/accounts");

            AddRequestHeaders(message);

            var response = await _client.SendAsync(message);

            var stringData = await response.Content.ReadAsStringAsync();

            var accounts = JsonSerializer.Deserialize<Account[]>(stringData);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var account in accounts)
            {
                var balance = decimal.Parse(account.Balance);

                if (balance > 0)
                {
                    balances.Add(new ExchangeBalance
                                 {
                                     Amount = balance,
                                     Currency = account.Currency
                                 });
                }
            }

            return balances;
        }
        
        private async Task<Dictionary<string, decimal>> GetExchangeRates(List<string> currencies)
        {
            var rates = new Dictionary<string, decimal>();

            foreach (var currency in currencies)
            {
                try
                {
                    // TODO: Some APIs don't support exchanges to all currencies, e.g. CBP doesn't have XLM-GBP.
                    // Use a fallback of USD, then convert to GBP?
                    var message = new HttpRequestMessage(HttpMethod.Get, $"/products/{currency.ToUpperInvariant()}-{_fiatCurrency}/ticker");

                    var response = await _client.SendAsync(message);

                    var stringData = await response.Content.ReadAsStringAsync();

                    var ticker = JsonSerializer.Deserialize<Ticker>(stringData);

                    // ReSharper disable once PossibleNullReferenceException
                    rates.Add(currency.ToUpperInvariant(), decimal.Parse(ticker.Price));
                }
                catch
                {
                    //
                }
            }

            return rates;
        }

        private void AddRequestHeaders(HttpRequestMessage message, string body = null)
        {

            var timestamp = $"{(long) DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds}";

            // ReSharper disable once PossibleNullReferenceException
            var toSign = $"{timestamp}{message.Method.ToString().ToUpper()}{message.RequestUri.OriginalString}{body ?? string.Empty}";

            var bytes = Encoding.ASCII.GetBytes(toSign);

            // ReSharper disable once IdentifierTypo
            using var hmacsha256 = new HMACSHA256(Convert.FromBase64String(_apiSecret));

            var hash = hmacsha256.ComputeHash(bytes);

            message.Headers.Add("CB-ACCESS-SIGN", Convert.ToBase64String(hash));
            message.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
        }
    }
}