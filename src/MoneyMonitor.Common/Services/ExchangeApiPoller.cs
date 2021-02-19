    using System;
using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Services
{
    public class ExchangeApiPoller
    {
        private readonly ILogger _logger;

        private readonly List<ICryptoExchangeClient> _exchangeClients;

        private readonly Action<List<ExchangeBalance>> _polled;

        private Thread _pollThread;

        public ExchangeApiPoller(ILogger logger, List<ICryptoExchangeClient> exchangeClients, Action<List<ExchangeBalance>> polled)
        {
            _logger = logger;

            _exchangeClients = exchangeClients;

            _polled = polled;
        }

        public void StartPolling(TimeSpan interval)
        {
            _pollThread = new Thread(async () => await Poll(interval))
                          {
                              IsBackground = true
                          };

            _pollThread.Start();
        }

        private async Task Poll(TimeSpan interval)
        {
            while (true)
            {
                try
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
                                                    
                                                });
                            }
                            else
                            {
                            }
                        }
                    }

                    _polled(allBalances);
                }
                catch (Exception exception)
                {
                    _logger.LogError("An error occurred polling the exchange API.", exception);
                }

                Thread.Sleep(interval);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}