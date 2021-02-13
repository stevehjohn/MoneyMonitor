using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Services
{
    public class HistoryManager
    {
        private readonly int _length;

        private readonly string _filename;

        private Queue<HistoryEntry> _history;

        public HistoryManager(int length, string filename)
        {
            _length = length;

            _filename = filename;

            _history = new Queue<HistoryEntry>(_length);
        }

        public void Save()
        {
            var json = JsonSerializer.Serialize(_history);

            File.WriteAllText(_filename, json);
        }

        public void Load()
        {
            if (! File.Exists(_filename))
            {
                return;
            }

            var json = File.ReadAllText(_filename);

            _history = JsonSerializer.Deserialize<Queue<HistoryEntry>>(json);
        }

        public void AddEntry(List<ExchangeBalance> balances)
        {
            if (_history.Count == _length)
            {
                _history.Dequeue();
            }

            _history.Enqueue(new HistoryEntry
                             {
                                 Balances = balances,
                                 Time = balances.Max(b => b.TimeUtc)
                             });
        }
    }
}