using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Services
{
    public class ExchangeAggregator
    {
        private readonly List<ICryptoExchangeClient> _exchangeClients;

        public ExchangeAggregator(List<ICryptoExchangeClient> exchangeClients)
        {
            _exchangeClients = exchangeClients;
        }

        public async Task<List<ExchangeBalance>> GetAllBalances()
        {
            var allBalances = new List<ExchangeBalance>();

            foreach (var client in _exchangeClients)
            {
                var balances = await client.GetBalances();

                foreach (var balance in balances)
                {
                    var item = allBalances.FirstOrDefault(b => b.Currency.Equals(balance.Currency, StringComparison.InvariantCultureIgnoreCase));

                    if (item == null)
                    {
                        allBalances.Add(new ExchangeBalance
                                        {
                                            Amount = balance.Amount,
                                            Currency = balance.Currency,
                                            ExchangeRate = balance.ExchangeRate,
                                            TimeUtc = balance.TimeUtc,
                                            Value = balance.Value
                                        });
                    }
                    else
                    {
                        item.Amount += balance.Amount;
                        item.Value += balance.Value;

                        if (balance.TimeUtc > item.TimeUtc)
                        {
                            item.ExchangeRate = balance.ExchangeRate;
                            item.TimeUtc = balance.TimeUtc;
                        }
                    }
                }
            }

            return allBalances;
        }
    }
}