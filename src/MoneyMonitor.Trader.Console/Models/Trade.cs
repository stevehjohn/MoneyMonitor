namespace MoneyMonitor.Trader.Console.Models
{
    public class Trade
    {
        public int Buys { get; set; }

        public decimal PreviousTradePrice { get; set; }

        public int Sells { get; set; }

        public Side Side { get; set; }

        public string LastTradeId { get; set; }
    }
}