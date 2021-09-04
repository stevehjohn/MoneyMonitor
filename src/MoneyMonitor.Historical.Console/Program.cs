using System;
using System.Threading.Tasks;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Historical.Console.Services;

namespace MoneyMonitor.Historical.Console
{
    class Program
    {
        private static CurrencyHistoryManager _history;

        public static async Task Main()
        {
            var logger = new FileLogger("history-errors.txt");

            _history = new CurrencyHistoryManager(logger);

            System.Console.CursorVisible = false;

            try
            {
                await _history.ExecuteAsync("BTC");
            }
            catch (Exception exception)
            {
                logger.LogError("An error occurred when calling the Trade method.", exception);
            }
        }
    }
}
