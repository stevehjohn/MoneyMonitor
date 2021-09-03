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

        private int _profit;

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
        }

        public async Task Trade(string currency)
        {
            currency = currency.ToUpperInvariant();

            var rates = await _client.GetExchangeRates(new List<string> { currency });

            var rate = rates[currency];

            _output.Write($"{DateTime.UtcNow:G},{currency},{rate:F2}");

            if (! _tradeInfos.ContainsKey(currency))
            {
                _tradeInfos.Add(currency, new Trade
                                          {
                                              PreviousTradePrice = rate,
                                              Side = Side.Buy
                                          });

                return;
            }

            var trade = _tradeInfos[currency];

            // TODO: Check if previous active trade. If expired/unfulfilled, reset the previous trade price to current price.

            if (trade.Side == Side.Buy)
            {
                if (trade.PreviousTradePrice - rate > 11)
                {
                    _output.Write("Buy.");

                    trade.PreviousTradePrice = rate;

                    trade.Side = Side.Sell;
                }

                return;
            }

            if (rate - trade.PreviousTradePrice > 31)
            {
                _output.Write($"Sell. Total profit {Settings.Instance.FiatCurrencySymbol}{_profit}");

                _profit += 10;

                trade.PreviousTradePrice = rate;

                trade.Side = Side.Buy;
            }
        }
    }
}