using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;

namespace MoneyMonitor.Common.Services
{
    public class FiatExchangeRateConverter
    {
        private readonly string _baseCurrency;

        private readonly TimeSpan _refreshInterval;

        private readonly FiatExchangeRateClient _exchangeRateClient;

        private Dictionary<string, decimal> _rates;

        private DateTime _rateAge = DateTime.MinValue;

        public FiatExchangeRateConverter(string baseCurrency, TimeSpan refreshInterval)
        {
            _baseCurrency = baseCurrency;

            _refreshInterval = refreshInterval;

            _exchangeRateClient = new FiatExchangeRateClient();
        }

        public async Task<decimal> GetValueInBaseCurrency(string currency, decimal value)
        {
            await RefreshRates();

            return value / _rates[currency];
        }

        private async Task RefreshRates()
        {
            if (DateTime.UtcNow - _rateAge < _refreshInterval)
            {
                return;
            }

            _rates = await _exchangeRateClient.GetExchangeRates(_baseCurrency);

            _rateAge = DateTime.UtcNow;
        }
    }
}