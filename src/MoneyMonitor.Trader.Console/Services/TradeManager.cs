using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Trader.Console.Infrastructure;
using MoneyMonitor.Trader.Console.Models;

namespace MoneyMonitor.Trader.Console.Services
{
    public class TradeManager
    {
        private readonly CoinbaseProExchangeClient _client;

        private readonly Dictionary<string, Trade> _tradeInfos;

        private readonly Output _output;

        public TradeManager(ILogger logger)
        {
            var settings = Settings.Instance;

            _client = new CoinbaseProExchangeClient(settings.CoinbaseProCredentials.ApiKey,
                                                    settings.CoinbaseProCredentials.ApiSecret,
                                                    settings.CoinbaseProCredentials.Passphrase,
                                                    settings.FiatCurrency,
                                                    null, 
                                                    null, 
                                                    logger);

            _tradeInfos = new Dictionary<string, Trade>();

            _output = new Output("trade-data.csv");

            _output.Write("DateTime,Crypto,Price,TargetDelta,BuyCount,SellCount,Action");
        }

        public async Task Trade(string currency)
        {
            currency = currency.ToUpperInvariant();

            var rates = await _client.GetExchangeRates(new List<string> { currency });

            var rate = rates[currency];

            if (! _tradeInfos.ContainsKey(currency))
            {
                _tradeInfos.Add(currency, new Trade
                                          {
                                              PreviousTradePrice = rate,
                                              Side = Side.Buy
                                          });

                WriteOut(currency, rate, 0, 0, 0, "INITIALISE");

                return;
            }

            var trade = _tradeInfos[currency];

            _output.Write($"{DateTime.UtcNow:G},{currency},{rate:F2},,{trade.Buys},{trade.Sells},POLL");

            var delta = 0;

            WriteOut(currency, rate, delta, trade.Buys, trade.Sells, "POLL");

            // TODO: Check if previous active trade. If expired/unfulfilled, reset the previous trade price to current price.

            if (trade.Side == Side.Buy)
            {
                if (trade.PreviousTradePrice - rate > 11)
                {
                    trade.PreviousTradePrice = rate;

                    trade.Buys++;

                    trade.Side = Side.Sell;

                    WriteOut(currency, rate, 0, trade.Buys, trade.Sells, "BUY");
                }

                return;
            }

            if (rate - trade.PreviousTradePrice > 31)
            {
                trade.PreviousTradePrice = rate;

                trade.Sells++;

                trade.Side = Side.Buy;

                WriteOut(currency, rate, 0, trade.Buys, trade.Sells, "SELL");
            }
        }

        private void WriteOut(string currency, decimal rate, decimal delta, int buys, int sells, string action)
        {
            _output.Write($"{DateTime.UtcNow:G},{currency},{rate:F2},{delta:F2},{buys},{sells},{action}");
        }
    }
}