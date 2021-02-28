using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using MoneyMonitor.Common.Models.FiatExchangeRateApiResponses;

namespace MoneyMonitor.Common.Clients
{
    public class FiatExchangeRateClient
    {
        private readonly HttpClient _client;

        public FiatExchangeRateClient()
        {
            _client = new HttpClient
                      {
                          BaseAddress = new Uri("https://api.exchangeratesapi.io")
                      };
        }

        public async Task<Dictionary<string, decimal>> GetExchangeRates(string baseCurrency)
        {
            var response = await _client.GetAsync($"latest?base={baseCurrency}");

            var stringData = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<RatesResponse>(stringData);

            // ReSharper disable once PossibleNullReferenceException
            return data.Rates;
        }
    }
}