namespace MoneyMonitor.Common.Models
{
    public class LastTrade
    {
        public decimal Price { get; set; }

        public bool Buy { get; set; }

        public decimal Cumulative { get; set; }
    }
}