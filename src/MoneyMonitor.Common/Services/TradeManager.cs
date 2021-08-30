using System;
using System.Collections.Generic;
using System.IO;

namespace MoneyMonitor.Common.Services
{
    public class TradeManager
    {
        private readonly HistoryManager _historyManager;

        private readonly Dictionary<string, decimal> _lastTradePrices;

        private bool _buy = true;

        public TradeManager(HistoryManager historyManager)
        {
            _historyManager = historyManager;

            _lastTradePrices = new Dictionary<string, decimal>();
        }

        public void Trade()
        {
            var price = 1 / _historyManager.GetExchangeRate("ETH");

            if (price == null)
            {
                return;
            }

            if (! _lastTradePrices.ContainsKey("ETH"))
            {
                _lastTradePrices.Add("ETH", (decimal) price);

                return;
            }

            if (_buy)
            {
                if (_lastTradePrices["ETH"] - price > 11)
                {
                    File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},BUY,{price}\n");

                    _lastTradePrices["ETH"] = (decimal) price;

                    _buy = false;
                }
                else
                {
                    File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},NO ACTION,{price}\n");
                    
                    return;
                }
            }

            if (price - _lastTradePrices["ETH"] > 21)
            {
                File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},SELL,{price}\n");

                _lastTradePrices["ETH"] = (decimal) price;

                _buy = true;
            }
            else
            {
                File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},NO ACTION,{price}\n");
            }
        }
    }
}