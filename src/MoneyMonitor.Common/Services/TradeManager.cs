using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Services
{
    public class TradeManager
    {
        private readonly HistoryManager _historyManager;

        private readonly Dictionary<string, LastTrade> _lastTradePrices;

        public TradeManager(HistoryManager historyManager)
        {
            _historyManager = historyManager;

            _lastTradePrices = new Dictionary<string, LastTrade>();
        }

        public void Trade()
        {
            Trade("ETH");
            
            Trade("BTC");
        }

        public void Trade(string currency)
        {
            var price = 1 / _historyManager.GetExchangeRate(currency);

            if (price == null)
            {
                return;
            }

            if (! _lastTradePrices.ContainsKey(currency))
            {
                _lastTradePrices.Add(currency, new LastTrade
                                               {
                                                   Buy = true,
                                                   Price = (decimal) price
                                               });

                return;
            }

            var buy = _lastTradePrices[currency].Buy;

            if (buy)
            {
                if (_lastTradePrices[currency].Price - price > 11)
                {
                    File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},BUY,£{price:F2},-£10\n", Encoding.UTF8);

                    _lastTradePrices[currency].Price = (decimal) price;

                    _lastTradePrices[currency].Buy = false;
                }
                else
                {
                    File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},NO ACTION,£{price:F2}\n", Encoding.UTF8);
                }
                    
                return;
            }

            if (price - _lastTradePrices[currency].Price > 21)
            {
                File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},SELL,£{price:F2},£10\n", Encoding.UTF8);

                _lastTradePrices[currency].Price = (decimal) price;

                _lastTradePrices[currency].Buy = true;
            }
            else
            {
                File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},NO ACTION,£{price:F2}\n", Encoding.UTF8);
            }
        }
    }
}