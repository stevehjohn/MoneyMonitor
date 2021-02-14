using System;
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

        public List<int> GetHistory(string currency = null)
        {
            var history = new List<int>();

            foreach (var entry in _history)
            {
                if (string.IsNullOrWhiteSpace(currency))
                {
                    history.Add(entry.Balances.Sum(b => b.Value));
                }
                else
                {
                    history.Add(entry.Balances.FirstOrDefault(b => b.Currency.Equals(currency, StringComparison.InvariantCultureIgnoreCase))?.Value ?? 0);
                }
            }

            return history;
        }

        public decimal GetExchangeRate(string currency)
        {
            // ReSharper disable once PossibleNullReferenceException
            return _history.Last().Balances.FirstOrDefault(b => b.Currency.Equals(currency, StringComparison.InvariantCultureIgnoreCase)).ExchangeRate;
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