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

        private readonly string _appId;

        public FiatExchangeRateClient(string appId)
        {
            _appId = appId;

            _client = new HttpClient
                      {
                          BaseAddress = new Uri("http://openexchangerates.org")
                      };
        }

        public async Task<Dictionary<string, decimal>> GetExchangeRates()
        {
            var response = await _client.GetAsync($"api/latest.json?app_id={_appId}");

            var stringData = await response.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<RatesResponse>(stringData);

            // ReSharper disable once PossibleNullReferenceException
            return data.Rates;
        }
    }
}