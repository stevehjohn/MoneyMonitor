using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Models.CoinbaseProApiResponses;
using MoneyMonitor.Common.Services;

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
            _exchangeRateConverter = exchangeRateConverter;
            _currencyOverrides = currencyOverrides;
            _logger = logger;
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
        
        private async Task<Dictionary<string, decimal>> GetExchangeRates(List<string> currencies)
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