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

            while (true)
            {
                await _trader.Trade("BTC");

                Thread.Sleep(Settings.Instance.PollInterval);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
