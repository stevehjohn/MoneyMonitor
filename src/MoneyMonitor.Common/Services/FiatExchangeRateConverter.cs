using System;
using System.Collections.Generic;
using MoneyMonitor.Common.Clients;

namespace MoneyMonitor.Common.Services
{
    public class FiatExchangeRateConverter
    {
        private readonly string _baseCurrency;

        private readonly FiatExchangeRateClient _exchangeRateClient;

        private Dictionary<string, decimal> _rates;

        private DateTime _rateAge = DateTime.MinValue;

        public FiatExchangeRateConverter(string baseCurrency)
        {
            _baseCurrency = baseCurrency;

            _exchangeRateClient = new FiatExchangeRateClient();
        }

        public decimal GetValueInBaseCurrency(string currency, decimal value)
        {
            RefreshRates();

            return value / _rates[currency];
        }

        private void RefreshRates()
        {
            // TODO: Make rates refresh age config parameter?
            if (DateTime.UtcNow - _rateAge < TimeSpan.FromHours(12))
            {
                return;
            }

            _rates = _exchangeRateClient.GetExchangeRates(_baseCurrency);

            _rateAge = DateTime.UtcNow;
        }
    }
}