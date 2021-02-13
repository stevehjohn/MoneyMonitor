using System;
using System.Collections.Generic;
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

        private readonly ICryptoExchangeClient _exchangeClient;

        private readonly Action<List<ExchangeBalance>> _polled;

        private Thread _pollThread;

        public ExchangeApiPoller(ILogger logger, ICryptoExchangeClient exchangeClient, Action<List<ExchangeBalance>> polled)
        {
            _logger = logger;

            _exchangeClient = exchangeClient;

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
                    var balances = await _exchangeClient.GetBalances();

                    _polled(balances);
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