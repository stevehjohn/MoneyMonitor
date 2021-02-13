using System;

namespace MoneyMonitor.Common.Models
{
    public class ExchangeBalance
    {
        public string Currency { get; set; }

        public decimal Amount { get;set; }

        public int Value { get; set; }

        public DateTime TimeUtc { get; set; }
    }
}