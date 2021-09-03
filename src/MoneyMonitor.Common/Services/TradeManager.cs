using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Services
{
    public class TradeManager
    {
        private readonly HistoryManager _historyManager;

        private readonly Dictionary<string, LastTrade> _lastTrades;

        // TODO: Use ICryptoExchangeClient
        private readonly CoinbaseProExchangeClient _exchangeClient;

        public TradeManager(HistoryManager historyManager, CoinbaseProExchangeClient exchangeClient)
        {
            _historyManager = historyManager;

            _exchangeClient = exchangeClient;

            _lastTrades = new Dictionary<string, LastTrade>();
        }

        public void Trade()
        {
            Trade("ETH").ConfigureAwait(false);
        }

        public async Task Trade(string currency)
        {
            var price = 1 / _historyManager.GetExchangeRate(currency);

            if (price == null)
            {
                return;
            }

            if (! _lastTrades.ContainsKey(currency))
            {
                _lastTrades.Add(currency, new LastTrade
                                          {
                                              Buy = true,
                                              Price = (decimal) price,
                                              Cumulative = 0
                                          });

                await File.AppendAllTextAsync("trades.csv", $"{DateTime.UtcNow:G},{currency},FIRST ENTRY,£{price:F2},£0,£0\n", Encoding.UTF8);

                return;
            }

            if (! string.IsNullOrEmpty(_lastTrades[currency].LastTradeId))
            {
                var status = await _exchangeClient.GetOrderStatus(_lastTrades[currency].LastTradeId);

                if (! new[] { "rejected", "done" }.Contains(status.Status.ToLowerInvariant()))
                {
                }
            }

            if (_lastTrades[currency].Buy)
            {
                if (_historyManager.GetHolding("GBP") < 50)
                {
                    await File.AppendAllTextAsync("trades.csv", "Insufficient funds for buy order.");

                    return;
                }

                if (_lastTrades[currency].Price - price > 11)
                {
                    _lastTrades[currency].Price = (decimal) price;

                    _lastTrades[currency].Cumulative -= 10;

                    _lastTrades[currency].Buy = false;

                    await File.AppendAllTextAsync("trades.csv", $"{DateTime.UtcNow:G},{currency},BUY,£{price:F2},-£10,£{_lastTrades[currency].Cumulative}\n", Encoding.UTF8);

                    var amount = 10 / price;

                    await File.AppendAllTextAsync("trades.csv", $"Placing buy order for {amount:F8} {currency} @ {price:F2}, cost {amount * price:F2}\n", Encoding.UTF8);

                    _lastTrades[currency].LastTradeId = await _exchangeClient.Trade(currency, (decimal) price, 10 / (decimal) price, true);
                }
                else
                {
                    await File.AppendAllTextAsync("trades.csv", $"{DateTime.UtcNow:G},{currency},NO ACTION,£{price:F2},£0,£{_lastTrades[currency].Cumulative}\n", Encoding.UTF8);
                }

                return;
            }

            if (price - _lastTrades[currency].Price > 31)
            {
                _lastTrades[currency].Price = (decimal) price;

                _lastTrades[currency].Cumulative += 20;

                _lastTrades[currency].Buy = true;

                await File.AppendAllTextAsync("trades.csv", $"{DateTime.UtcNow:G},{currency},SELL,£{price:F2},£10,£{_lastTrades[currency].Cumulative}\n", Encoding.UTF8);

                var amount = 10 / price;

                await File.AppendAllTextAsync("trades.csv", $"Placing sell order for {20 / price:F8} {currency} @ {price:F2}, cost {amount * price:F2}\n", Encoding.UTF8);

                _lastTrades[currency].LastTradeId = await _exchangeClient.Trade(currency, (decimal) price, 20 / (decimal) price, false);
            }
            else
            {
                await File.AppendAllTextAsync("trades.csv", $"{DateTime.UtcNow:G},{currency},NO ACTION,£{price:F2},£0,£{_lastTrades[currency].Cumulative}\n", Encoding.UTF8);
            }
        }
    }
}