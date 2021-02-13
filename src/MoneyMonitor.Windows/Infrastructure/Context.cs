using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MoneyMonitor.Common.Clients;
using MoneyMonitor.Common.Infrastructure;
using MoneyMonitor.Common.Models;
using MoneyMonitor.Common.Services;
using MoneyMonitor.Windows.Infrastructure.Settings;
using MoneyMonitor.Windows.Services;

namespace MoneyMonitor.Windows.Infrastructure
{
    public class Context : ApplicationContext
    {
        private readonly HistoryManager _historyManager;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable - don't want it GC'd when constructor completes.
        private readonly ExchangeApiPoller _poller;

        private readonly FormManager _formManager;

        private readonly TrayManager _trayManager;

        public Context()
        {
            var settings = AppSettings.Instance;

            var client = new CoinbaseExchangeClient(settings.CoinbaseCredentials.ApiKey, settings.CoinbaseCredentials.ApiSecret, settings.FiatCurrency);

            var logger = new FileLogger(Constants.LogFilename);

            _historyManager = new HistoryManager(Constants.HistoryLength, Constants.HistoryFilename);

            _historyManager.Load();

            _trayManager = new TrayManager
                           {
                               ExitClicked = ExitClicked,
                               IconClicked = IconClicked
                           };

            _formManager = new FormManager(_historyManager);

            _poller = new ExchangeApiPoller(logger, client, Polled);

            _poller.StartPolling(settings.PollInterval);
        }

        private void Polled(List<ExchangeBalance> balances)
        {
            _historyManager.AddEntry(balances);

            _historyManager.Save();

            _trayManager.BalanceChanged(balances.Sum(b => b.Value));

            _formManager.NewData();
        }

        private void ExitClicked()
        {
            _trayManager.HideTrayIcon();

            Application.Exit();
        }

        private void IconClicked()
        {
            _formManager.ShowHistory();
        }
    }
}