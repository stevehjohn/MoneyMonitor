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
                _lastTradePrices.Add(currency, (decimal) price);

                return;
            }

            if (_buy)
            {
                if (_lastTradePrices[currency] - price > 11)
                {
                    File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},{currency},BUY,{price:C},-£10\n");

                    _lastTradePrices[currency] = (decimal) price;

                    _buy = false;
                }
                else
                {
                    File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},{currency},NO ACTION,{price:C}\n");
                }
                    
                return;
            }

            if (price - _lastTradePrices[currency] > 21)
            {
                File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},{currency},SELL,{price:C},£10\n");

                _lastTradePrices[currency] = (decimal) price;

                _buy = true;
            }
            else
            {
                File.AppendAllText("trades.txt", $"{DateTime.UtcNow:G},{currency},NO ACTION,{price:C}\n");
            }
        }
    }
}