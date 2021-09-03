namespace MoneyMonitor.Trader.Console.Models
{
    public class Trade
    {
        public decimal PreviousTradePrice { get; set; }

        public Side Side { get; set; }
    }
}