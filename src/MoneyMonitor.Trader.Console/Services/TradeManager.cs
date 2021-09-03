using System;
using System.Collections.Generic;
using System.Linq;
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

            _output.Write("DateTime,Crypto,Price,TargetDelta,BuyCount,SellCount,Action", ConsoleColor.White);
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

                WriteOut(currency, rate, 0, 0, 0, "INITIALISE", ConsoleColor.Gray);

                return;
            }

            var trade = _tradeInfos[currency];

            var delta = trade.PreviousTradePrice - rate;

            if (! string.IsNullOrWhiteSpace(trade.LastTradeId))
            {
                var status = await _client.GetOrderStatus(trade.LastTradeId);

                if (status != null)
                {
                    if (new[] { "pending", "active", "open" }.Contains(status.Status.ToLowerInvariant()))
                    {
                        WriteOut(currency, rate, delta, trade.Buys, trade.Sells, "ACTIVE TRADE", ConsoleColor.DarkCyan);

                        return;
                    }
                    
                    if (new[] { "done", "settled" }.Contains(status.Status.ToLowerInvariant()))
                    {
                        WriteOut(currency, rate, delta, trade.Buys, trade.Sells, "TRADE COMPLETE", ConsoleColor.Blue);

                        trade.PreviousTradePrice = rate;

                        trade.LastTradeId = null;

                        return;
                    }
                }

                // TODO: Trade complete?
                WriteOut(currency, rate, delta, trade.Buys, trade.Sells, "TRADE CANCELLED", ConsoleColor.Blue);

                trade.PreviousTradePrice = rate;

                trade.LastTradeId = null;

                return;
            }

            WriteOut(currency, rate, delta, trade.Buys, trade.Sells, "POLL", ConsoleColor.Gray);

            if (trade.Side == Side.Buy)
            {
                if (trade.PreviousTradePrice - rate > 11)
                {
                    trade.LastTradeId = await _client.Trade(currency, rate, 10 / rate, true);

                    trade.PreviousTradePrice = rate;

                    trade.Buys++;

                    trade.Side = Side.Sell;

                    WriteOut(currency, rate, 0, trade.Buys, trade.Sells, "BUY", ConsoleColor.Red);
                }

                return;
            }

            if (rate - trade.PreviousTradePrice > 31)
            {
                trade.LastTradeId = await _client.Trade(currency, rate, 20 / rate, false);

                trade.PreviousTradePrice = rate;

                trade.Sells++;

                trade.Side = Side.Buy;

                WriteOut(currency, rate, 0, trade.Buys, trade.Sells, "SELL", ConsoleColor.Green);
            }
        }

        private void WriteOut(string currency, decimal rate, decimal delta, int buys, int sells, string action, ConsoleColor colour)
        {
            _output.Write($"{DateTime.UtcNow:G},{currency},{rate:F2},{delta:F2},{buys},{sells},{action}", colour);
        }
    }
}