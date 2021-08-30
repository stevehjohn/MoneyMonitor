namespace MoneyMonitor.Windows.Infrastructure.Settings
{
    public class Trade
    {
        public string Exchange { get; set; }

        public string CryptoCurrency { get; set; }

        public decimal BuyThreshold { get; set; }

        public decimal SellThreshold { get; set; }

        public decimal BuyAmount { get; set; }

        public decimal SellAmount { get; set; }
    }
}