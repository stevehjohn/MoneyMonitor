using MoneyMonitor.Trader.Console.Models;

namespace MoneyMonitor.Trader.Console.Infrastructure.Settings
{
    public class TradeParameters
    {
        public string Currency { get; set; }

        public decimal BaseAmount { get; set; }

        public decimal BuyDropThreshold { get; set; }

        public decimal SellRiseThreshold { get; set; }

        public Side? LastSide { get; set; }

        public decimal? LastTradePrice { get; set; }
    }
}