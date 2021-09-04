﻿using System;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Trader.Console.Services;
using System.Threading;
using System.Threading.Tasks;

namespace MoneyMonitor.Trader.Console.Infrastructure
{
    public class StartUp
    {
        private static TradeManager _trader;

        public static async Task Main()
        {
            var logger = new FileLogger("trade-errors.txt");

            _trader = new TradeManager(logger);

            System.Console.CursorVisible = false;

            var tradeParameters = Settings.Settings.Instance.TradeParameters;

            while (true)
            {
                try
                {
                    foreach (var trade in tradeParameters)
                    {
                        await _trader.Trade(trade);
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError("An error occurred when calling the Trade method.", exception);
                }

                Thread.Sleep(Settings.Settings.Instance.PollInterval);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
