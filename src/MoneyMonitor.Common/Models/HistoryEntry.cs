using System;
using System.Collections.Generic;

namespace MoneyMonitor.Common.Models
{
    public class HistoryEntry
    {
        public DateTime Time { get; set; }

        public List<ExchangeBalance> Balances { get; set; }
    }
}