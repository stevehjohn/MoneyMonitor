using System.Collections.Generic;

namespace MoneyMonitor.Common.Models
{
    public class History
    {
        public Queue<HistoryEntry> HistoryEntries { get; set; }

        public List<HistorySummary> HistorySummaries { get; set; }
    }
}