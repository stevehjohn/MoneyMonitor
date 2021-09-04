using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Models.CoinbaseProApiRequests;
using MoneyMonitor.Common.Models.CoinbaseProApiResponses;
using MoneyMonitor.Common.Services;
using Newtonsoft.Json.Linq;

namespace MoneyMonitor.Common.Clients
{
    public class CoinbaseProExchangeClient : ICryptoExchangeClient
    {
        private readonly HttpClient _client;

        private readonly string _apiSecret;

        private readonly string _fiatCurrency;

        private readonly FiatExchangeRateConverter _exchangeRateConverter;

        private readonly Dictionary<string, string> _currencyOverrides;

        private readonly ILogger _logger;

        public CoinbaseProExchangeClient(string apiKey, string apiSecret, string passphrase, string fiatCurrency, FiatExchangeRateConverter exchangeRateConverter, Dictionary<string, string> currencyOverrides, ILogger logger)
        {
            HttpClientHandler httpClientHandler = new HttpClientHandler()
            {
                Proxy = new WebProxy(string.Format("{0}:{1}", "127.0.0.1", 8888), false)
            };

            _client = new HttpClient(httpClientHandler)
                      {
                          BaseAddress = new Uri("https://api.pro.coinbase.com"),
                      };

            _client.DefaultRequestHeaders.Add("User-Agent", "CoinbaseProApiClient");
            _client.DefaultRequestHeaders.Add("Accept", "application/json");
            _client.DefaultRequestHeaders.Add("CB-ACCESS-KEY", apiKey);
            _client.DefaultRequestHeaders.Add("CB-ACCESS-PASSPHRASE", passphrase);

            _apiSecret = apiSecret;
            _fiatCurrency = fiatCurrency;
            _exchangeRateConverter = exchangeRateConverter;
            _currencyOverrides = currencyOverrides;
            _logger = logger;
        }

        // [[1630794600,36025.63,36265.88,36265.88,36055.25,10.0927808],[1630793700,36194.45,36304.58,36283.04,36257.95,7.84832646],[ ...
        [DebuggerDisplay("{time}")]
        public sealed class HistoryResponse
        {
            /// <summary>
            ///     Timestamp to DateTime:
            ///     var xx = DateTime.UnixEpoch + TimeSpan.FromSeconds(1630795500); // 4th September 2021 @ 22:00 ish
            /// </summary>
            public int time;
            public double low;
            public double high;
            public double open;
            public double close;
            public double volume;
        }

        /// <summary>
        ///     Historical Price Data
        /// </summary>
        /// <param name="currency">Currency Pair - e.g. BTC-GBP</param>
        /// <param name="granularity">The granularity field must be one of the following values: {60, 300, 900, 3600, 21600, 86400} (seconds)</param>
        /// <returns><see cref="HistoryResponse"/></returns>
        public async Task<HistoryResponse[]> GetHistory(string currency, DateTime start, DateTime end, string granularity = "900")
        {
            string productId = $"{currency}-{_fiatCurrency}".ToUpperInvariant();

            string url = $"/products/{productId}/candles?start={start.ToISO8601()}&end={end.ToISO8601()}&granularity={granularity}";
            HttpRequestMessage message = new (HttpMethod.Get, url);
            AddRequestHeaders(message, null);

            var response = await _client.SendAsync(message);
            var stringData = await response.Content.ReadAsStringAsync();

            JArray jsonData = JArray.Parse(stringData);

            List<HistoryResponse> data = new List<HistoryResponse>();

            foreach (JArray item in jsonData)
            {
                data.Add(new HistoryResponse
                {
                    time = item.Value<int>(0),
                    low = item.Value<double>(1),
                    high = item.Value<double>(2),
                    open = item.Value<double>(3),
                    close = item.Value<double>(4),
                    volume = item.Value<double>(5)
                });
            }

            return data.ToArray();
        }

        public async Task<string> Trade(string currency, decimal price, decimal size, bool buy)
        {
            var request = new PlaceOrder
                          {
                              CancelAfter = "min",
                              OrderId = Guid.NewGuid().ToString("D"),
                              Price = price.ToString("F2", CultureInfo.InvariantCulture),
                              ProductId = $"{currency}-{_fiatCurrency}".ToUpperInvariant(),
                              Side = buy ? "buy" : "sell",
                              Size = size.ToString("F8", CultureInfo.InvariantCulture),
                              Stop = buy ? "loss" : "entry",
                              StopPrice = price.ToString("F2", CultureInfo.InvariantCulture),
                              TimeInForce = "GTT",
                              Type = "limit"
                          };

            var body = JsonSerializer.Serialize(request);

            var message = new HttpRequestMessage(HttpMethod.Post, "/orders")
                          {
                              Content = new StringContent(body, Encoding.UTF8, "application/json")
                          };

            AddRequestHeaders(message, body);

            var response = await _client.SendAsync(message);

            response.EnsureSuccessStatusCode();

            // TODO: Log response?
            // var stringData = await response.Content.ReadAsStringAsync();

            return request.OrderId;
        }

        public async Task<OrderStatus> GetOrderStatus(string orderId)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, $"/orders/client:{orderId}");

            AddRequestHeaders(message);

            var response = await _client.SendAsync(message);

            var stringData = await response.Content.ReadAsStringAsync();

            var status = JsonSerializer.Deserialize<OrderStatus>(stringData);

            return status;
        }

        public async Task<List<ExchangeBalance>> GetBalances()
        {
            var balances = await GetCoinBalances();

            // TODO: Sort this out
            balances = balances.Where(b => b.Currency != "GBP").ToList();

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
        
        public async Task<Dictionary<string, decimal>> GetExchangeRates(List<string> currencies)
        {
            var rates = new Dictionary<string, decimal>();

            string pair = null;

            foreach (var currency in currencies)
            {
                try
                {
                    var currencyOverride = _currencyOverrides?.ContainsKey(currency.ToUpperInvariant()) ?? false;

                    var fiatCurrency = currencyOverride
                        ? _currencyOverrides[currency]
                        : _fiatCurrency;

                    pair = $"{currency.ToUpperInvariant()}-{fiatCurrency}";

                    var message = new HttpRequestMessage(HttpMethod.Get, $"/products/{pair}/ticker");

                    var response = await _client.SendAsync(message);

                    var stringData = await response.Content.ReadAsStringAsync();

                    var ticker = JsonSerializer.Deserialize<Ticker>(stringData);

                    // ReSharper disable once PossibleNullReferenceException
                    var price = decimal.Parse(ticker.Price);

                    if (currencyOverride)
                    {
                        price = await _exchangeRateConverter.GetValueInBaseCurrency(fiatCurrency, price);
                    }

                    rates.Add(currency.ToUpperInvariant(), price);
                }
                catch (Exception exception)
                {
                    _logger.LogError($"Error getting exchange rate for {pair}.", exception);
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