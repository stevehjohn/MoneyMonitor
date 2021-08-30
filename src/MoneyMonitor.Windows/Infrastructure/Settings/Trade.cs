namespace MoneyMonitor.Windows.Infrastructure.Settings
{
    public class Trade
    {
        public string Exchange { get; set; }

        public string CryptoCurrency { get; set; }

        public decimal Buy { get; set; }

        public decimal Sell { get; set; }
    }
}