﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Services;
using MoneyMonitor.Windows.Exceptions;
using MoneyMonitor.Windows.Infrastructure.Settings;
using MoneyMonitor.Windows.Services;
using OfficeOpenXml;

namespace MoneyMonitor.Windows.Infrastructure
{
    public class Context : ApplicationContext
    {
        private readonly HistoryManager _historyManager;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable - don't want it GC'd when constructor completes.
        private readonly ExchangeApiPoller _poller;

        private readonly FormManager _formManager;

        private readonly TrayManager _trayManager;

        private readonly ExchangeAggregator _exchangeAggregator;

        public Context()
        {
            var settings = AppSettings.Instance;

            var clients = settings.Clients.Split(',');

            var exchangeClients = new List<ICryptoExchangeClient>();

            foreach (var client in clients)
            {
                switch (client.Trim().ToLowerInvariant())
                {
                    case "coinbaseexchangeclient":
                        exchangeClients.Add(new CoinbaseExchangeClient(settings.CoinbaseCredentials.ApiKey, settings.CoinbaseCredentials.ApiSecret, settings.FiatCurrency));
                        break;
                    case "coinbaseproexchangeclient":
                        exchangeClients.Add(new CoinbaseProExchangeClient(settings.CoinbaseProCredentials.ApiKey, settings.CoinbaseProCredentials.ApiSecret, settings.CoinbaseProCredentials.Passphrase, settings.FiatCurrency));
                        break;
                    case "binanceexchangeclient":
                        exchangeClients.Add(new BinanceExchangeClient(settings.BinanceCredentials.ApiKey, settings.BinanceCredentials.SecretKey, settings.FiatCurrency));
                        break;
                    default:
                        throw new MoneyMonitorConfigurationException($"Unknown API client {client}.");
                }
            }

            var logger = new FileLogger(Constants.LogFilename);

            _historyManager = new HistoryManager(Constants.HistoryLength, Constants.HistoryFilename);

            _historyManager.Load();

            _trayManager = new TrayManager
                           {
                               ExitClicked = ExitClicked,
                               IconClicked = IconClicked,
                               TopMostToggled = TopMostToggled,
                               ShowCurrencyHistoryClicked = ShowCurrencyHistoryClicked,
                               HideCurrencyHistoryClicked = HideCurrencyHistoryClicked,
                               RefreshClicked = RefreshClicked
                           };

            _formManager = new FormManager(_historyManager);

            _exchangeAggregator = new ExchangeAggregator(exchangeClients);

            _poller = new ExchangeApiPoller(logger, _exchangeAggregator, Polled);

            _poller.StartPolling(settings.PollInterval);
        }

        private async void RefreshClicked()
        {
            var balances = await _exchangeAggregator.GetAllBalances();

            _historyManager.AddEntry(balances);

            _formManager.NewData();
        }

        private void HideCurrencyHistoryClicked(string currency)
        {
            _formManager.CloseForm(currency);
        }

        private void ShowCurrencyHistoryClicked(string currency)
        {
            _formManager.ShowHistory(false, currency);
        }

        private void TopMostToggled(bool topMost)
        {
            _formManager.UpdateTopMost(topMost);
        }

        private void Polled(List<ExchangeBalance> balances)
        {
            _historyManager.AddEntry(balances);

            _historyManager.Save();

            var balance = balances.Sum(b => b.Value);

            if (balance > AppSettings.Instance.BalanceHigh)
            {
                AppSettings.Instance.BalanceHigh = balance;

                AppSettings.Instance.Save();
            }
            else if (balance < AppSettings.Instance.BalanceLow)
            {
                AppSettings.Instance.BalanceLow = balance;

                AppSettings.Instance.Save();
            }

            _trayManager.BalanceChanged(balance);

            _trayManager.ConstructContextMenu(balances.Select(b => b.Currency).ToList());

            _formManager.NewData();

            UpdateExcel(balance);
        }

        private static void UpdateExcel(int balance)
        {
            if (string.IsNullOrWhiteSpace(AppSettings.Instance.ExcelFilePath) || string.IsNullOrWhiteSpace(AppSettings.Instance.ExcelCell))
            {
                return;
            }

            try
            {
                using var package = new ExcelPackage(new FileInfo(AppSettings.Instance.ExcelFilePath));

                var sheet = package.Workbook.Worksheets[0];

                var cell = sheet.Cells[AppSettings.Instance.ExcelCell];

                cell.Style.Numberformat.Format = "£#,###,##0.00";

                cell.Value = balance / 100m;

                package.Save();
            }
            catch
            {
                //
            } 
        }

        private void ExitClicked()
        {
            _trayManager.HideTrayIcon();

            Application.Exit();
        }

        private void IconClicked()
        {
            _formManager.ShowHistory(true);
        }
    }
}