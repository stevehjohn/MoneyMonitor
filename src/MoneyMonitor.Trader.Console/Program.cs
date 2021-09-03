using System;
using System.Threading;
using MoneyMonitor.Common.Clients;

namespace MoneyMonitor.Trader.Console
{
    public class Program
    {
        private static CoinbaseProExchangeClient _client;

        public static void Main()
        {
            _client = new CoinbaseProExchangeClient()

            while (true)
            {
                Trade("BTC");

                Thread.Sleep(new TimeSpan(0, 0, 10));
            }
        }

        private static void Trade(string currency)
        {
        }
    }
}
