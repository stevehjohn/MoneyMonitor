using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Models.CoinbaseApiResponses;

namespace MoneyMonitor.Common.Clients
{
    public class CoinbaseExchangeClient : ICryptoExchangeClient
    {
        private readonly HttpClient _client;

        private readonly string _apiSecret;
        private readonly string _fiatCurrency;

        public CoinbaseExchangeClient(string apiKey, string apiSecret, string fiatCurrency)
        {
            _client = new HttpClient
                      {
                          BaseAddress = new Uri("https://api.coinbase.com")
                      };

            _apiSecret = apiSecret;
            _fiatCurrency = fiatCurrency;

            _client.DefaultRequestHeaders.Add("CB-ACCESS-KEY", apiKey);
        }

        public async Task<List<ExchangeBalance>> GetBalances()
        {
            var balances = await GetCoinBalances();

            var result = new List<ExchangeBalance>();

            if (balances.Count == 0)
            {
                return result;
            }

            var exchangeRates = await GetExchangeRates();

            var now = DateTime.UtcNow;

            foreach (var coinBalance in balances)
            {
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

            PaginatedResponse<Account> data = null;

            do
            {
                var message = new HttpRequestMessage(HttpMethod.Get, data?.Pagination?.NextUri ?? "/v2/accounts");

                AddRequestHeaders(message);

                var response = await _client.SendAsync(message);

                var stringData = await response.Content.ReadAsStringAsync();

                data = JsonSerializer.Deserialize<PaginatedResponse<Account>>(stringData);

                // ReSharper disable once PossibleNullReferenceException
                foreach (var account in data.Data)
                {
                    var balance = decimal.Parse(account.Balance.Amount);

                    if (balance > 0)
                    {
                        balances.Add(new ExchangeBalance
                                     {
                                         Amount = balance,
                                         Currency = account.Balance.Currency
                                     });
                    }
                }

                Thread.Sleep(500);
            } while (! string.IsNullOrWhiteSpace(data.Pagination.NextUri));

            return balances;
        }
        
        private async Task<Dictionary<string, decimal>> GetExchangeRates()
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"/v2/exchange-rates?currency={_fiatCurrency}");

            var response = await _client.SendAsync(message);

            var stringData = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<DataResponse<RatesDictionary>>(stringData);

            var rates = new Dictionary<string, decimal>();

            // ReSharper disable once PossibleNullReferenceException
            foreach (var rate in data.Data.Rates)
            {
                rates.Add(rate.Key, decimal.Parse(rate.Value, NumberStyles.Any));
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
            using var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(_apiSecret));

            var hash = hmacsha256.ComputeHash(bytes);

            message.Headers.Add("CB-ACCESS-SIGN", BitConverter.ToString(hash).Replace("-", string.Empty).ToLower());
            message.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
        }
    }
}