using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Services
{
    public class ExchangeApiPoller
    {
        private readonly ILogger _logger;

        private readonly ExchangeAggregator _exchangeAggregator;

        private readonly Action<List<ExchangeBalance>> _polled;

        private Thread _pollThread;

        public ExchangeApiPoller(ILogger logger, ExchangeAggregator exchangeAggregator, Action<List<ExchangeBalance>> polled)
        {
            _logger = logger;

            _exchangeAggregator = exchangeAggregator;

            _polled = polled;
        }

        public void StartPolling(TimeSpan interval)
        {
            async void Start() => await Poll(interval);

            _pollThread = new Thread(Start)
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
                    _polled(await _exchangeAggregator.GetAllBalances());
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