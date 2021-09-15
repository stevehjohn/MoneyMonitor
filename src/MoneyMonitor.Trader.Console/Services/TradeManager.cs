using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Trader.Console.Infrastructure;
using MoneyMonitor.Trader.Console.Infrastructure.Settings;
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
            var settings = ConsoleSettings.Instance;

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

        public async Task Trade(TradeParameters parameters)
        {
            var currency = parameters.Currency.ToUpperInvariant();

            var rates = await _client.GetExchangeRates(new List<string> { currency });

            var rate = rates[currency];

            var currencySettings = ConsoleSettings.Instance.TradeParameters.First(p => p.Currency.Equals(currency, StringComparison.InvariantCultureIgnoreCase));

            if (! _tradeInfos.ContainsKey(currency))
            {
                _tradeInfos.Add(currency, new Trade
                                          {
                                              PreviousTradePrice = currencySettings.LastTradePrice ?? rate,
                                              Side = currencySettings.LastSide ?? Side.Sell
                                          });

                WriteOut(currency, rate, 0, 0, 0, "INITIALISE", ConsoleColor.Gray);

                return;
            }

            var trade = _tradeInfos[currency];

            var delta = trade.PreviousTradePrice - rate;

            if (! string.IsNullOrWhiteSpace(trade.LastTradeId))
            {
                var status = await _client.GetOrderStatus(trade.LastTradeId);

                if (status != null && ! string.IsNullOrWhiteSpace(status.Status))
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

                        currencySettings.LastTradePrice = rate;

                        currencySettings.LastSide = trade.Side;

                        ConsoleSettings.Instance.Save();

                        return;
                    }
                }

                WriteOut(currency, rate, delta, trade.Buys, trade.Sells, "TRADE CANCELLED", ConsoleColor.Blue);

                trade.PreviousTradePrice = rate;

                trade.LastTradeId = null;

                trade.Side = trade.Side == Side.Buy ? Side.Sell : Side.Buy;

                if (trade.Side == Side.Buy)
                {
                    trade.Buys--;
                }
                else
                {
                    trade.Sells--;
                }

                return;
            }

            WriteOut(currency, rate, delta, trade.Buys, trade.Sells, "POLL", ConsoleColor.Gray);

            if (trade.Side == Side.Buy)
            {
                if (trade.PreviousTradePrice - rate > parameters.BuyDropThreshold)
                {
                    trade.LastTradeId = await _client.Trade(currency, rate, parameters.BuyAmount, true);

                    trade.PreviousTradePrice = rate;

                    trade.Buys++;

                    trade.Side = Side.Sell;

                    WriteOut(currency, rate, 0, trade.Buys, trade.Sells, "BUY", ConsoleColor.Red);
                }

                return;
            }

            if (rate - trade.PreviousTradePrice > parameters.SellRiseThreshold)
            {
                trade.LastTradeId = await _client.Trade(currency, rate, parameters.SellAmount, false);

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