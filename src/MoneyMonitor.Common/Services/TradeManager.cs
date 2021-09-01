using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Models;

namespace MoneyMonitor.Common.Services
{
    public class TradeManager
    {
        private readonly HistoryManager _historyManager;

        private readonly Dictionary<string, LastTrade> _lastTradePrices;

        // TODO: Use ICryptoExchangeClient
        private readonly CoinbaseProExchangeClient _exchangeClient;

        public TradeManager(HistoryManager historyManager, CoinbaseProExchangeClient exchangeClient)
        {
            _historyManager = historyManager;

            _exchangeClient = exchangeClient;

            _lastTradePrices = new Dictionary<string, LastTrade>();
        }

        public void Trade()
        {
            Trade("ETH").Wait();
        }

        public async Task Trade(string currency)
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
                                                   Price = (decimal) price,
                                                   Cumulative = 0
                                               });

                File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},FIRST ENTRY,£{price:F2},£0,£0\n", Encoding.UTF8);

                return;
            }

            var buy = _lastTradePrices[currency].Buy;

            if (buy)
            {
                if (_historyManager.GetHolding("GBP") < 50)
                {
                    File.AppendAllText("trades.csv", "Insufficient funds for buy order.");

                    return;
                }

                if (_lastTradePrices[currency].Price - price > 11)
                {
                    _lastTradePrices[currency].Price = (decimal) price;

                    _lastTradePrices[currency].Cumulative -= 10;

                    _lastTradePrices[currency].Buy = false;

                    File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},BUY,£{price:F2},-£10,£{_lastTradePrices[currency].Cumulative}\n", Encoding.UTF8);
                    
                    File.AppendAllText("trades.csv", $"Placing buy order for {10 / price} {currency} @ {price / 10}\n", Encoding.UTF8);

                    await _exchangeClient.Trade(currency, (decimal) price / 10, 10 / (decimal) price, true);
                }
                else
                {
                    File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},NO ACTION,£{price:F2},£0,£{_lastTradePrices[currency].Cumulative}\n", Encoding.UTF8);
                }
                    
                return;
            }

            if (price - _lastTradePrices[currency].Price > 31)
            {
                _lastTradePrices[currency].Price = (decimal) price;

                _lastTradePrices[currency].Cumulative += 20;

                _lastTradePrices[currency].Buy = true;

                File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},SELL,£{price:F2},£10,£{_lastTradePrices[currency].Cumulative}\n", Encoding.UTF8);

                File.AppendAllText("trades.csv", $"Placing sell order for {20 / price} {currency} @ {price / 20}\n", Encoding.UTF8);
            }
            else
            {
                File.AppendAllText("trades.csv", $"{DateTime.UtcNow:G},{currency},NO ACTION,£{price:F2},£0,£{_lastTradePrices[currency].Cumulative}\n", Encoding.UTF8);
            }
        }
    }
}