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

        private List<HistorySummary> _historySummaries;

        public HistoryManager(int length, string filename)
        {
            _length = length;

            _filename = filename;

            _history = new Queue<HistoryEntry>(_length);

            _historySummaries = new List<HistorySummary>();
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

        public DateTime? GetHistoryTime()
        {
            return _history.LastOrDefault()?.Time;
        }

        public decimal? GetExchangeRate(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            return _history.Last().Balances.FirstOrDefault(b => b.Currency.Equals(currency, StringComparison.InvariantCultureIgnoreCase)).ExchangeRate;
        }

        public decimal? GetHolding(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            return _history.Last().Balances.FirstOrDefault(b => b.Currency.Equals(currency, StringComparison.InvariantCultureIgnoreCase)).Amount;
        }

        public void Save()
        {
            var history = new History
                          {
                              HistoryEntries = _history,
                              HistorySummaries = _historySummaries
                          };

            var json = JsonSerializer.Serialize(history);

            File.WriteAllText(_filename, json);
        }

        public void Load()
        {
            if (! File.Exists(_filename))
            {
                return;
            }

            var json = File.ReadAllText(_filename);

            var history = JsonSerializer.Deserialize<History>(json);

            if (history == null)
            {
                return;
            }

            _history = history.HistoryEntries;

            _historySummaries = history.HistorySummaries;
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

            foreach (var balance in balances)
            {
                var summary = _historySummaries.FirstOrDefault(s => s.Currency.Equals(balance.Currency));

                if (summary == null)
                {
                    summary = new HistorySummary
                              {
                                  Currency = balance.Currency,
                                  High = int.MinValue,
                                  Low = int.MaxValue
                              };

                    _historySummaries.Add(summary);
                }

                if (balance.Value > summary.High)
                {
                    summary.High = balance.Value;
                }

                if (balance.Value < summary.Low)
                {
                    summary.Low = balance.Value;
                }
            }
        }
    }
}