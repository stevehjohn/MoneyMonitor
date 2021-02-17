using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MoneyMonitor.Windows.Extensions;
using MoneyMonitor.Windows.Infrastructure.Settings;
using MoneyMonitor.Windows.Resources;

namespace MoneyMonitor.Windows.Services
{
    public class TrayManager
    {
        private readonly NotifyIcon _icon;

        private int _previousBalance;

        private readonly ContextMenuStrip _contextMenu;

        private ToolStripMenuItem _alwaysOnTop;
        
        private ToolStripMenuItem _allCurrencies;

        private delegate void SafeCallDelegate(List<string> currencies);

        public Action ExitClicked { set; private get; }

        public Action IconClicked { set; private get; }

        public Action<bool> TopMostToggled { set; private get; }

        public Action<string> ShowCurrencyHistoryClicked { set; private get; }
        
        public Action<string> HideCurrencyHistoryClicked { set; private get; }

        public Action RefreshClicked { set; private get; }

        public TrayManager()
        {
            _contextMenu = new ContextMenuStrip();

            _icon = new NotifyIcon
                    {
                        ContextMenuStrip = _contextMenu,
                        Icon = Icons.right,
                        Visible = true
                    };

            _icon.Click += TrayIconClicked;

            ConstructContextMenu(null);
        }

        public void BalanceChanged(int newBalance)
        {
            var settings = AppSettings.Instance;

            if (newBalance > _previousBalance)
            {
                _icon.Icon = newBalance > settings.BalanceHigh
                    ? Icons.up_green 
                    : Icons.up;
            }
            else if (newBalance < _previousBalance)
            {
                _icon.Icon = newBalance < settings.BalanceLow
                    ? Icons.down_red
                    : Icons.down;
            }
            else
            {
                _icon.Icon = Icons.right;
            }

            var symbol = settings.FiatCurrencySymbol;

            var low = AppSettings.Instance.BalanceLow == int.MaxValue
                ? 0
                : AppSettings.Instance.BalanceLow;

            // ReSharper disable once LocalizableElement
            _icon.Text = $"{DateTime.Now:HH:mm}\r\n\r\n🡅 {symbol}{AppSettings.Instance.BalanceHigh / 100m:N2}\r\n🡆 {symbol}{newBalance / 100m:N2}{Difference(newBalance)}\r\n🡇 {symbol}{low / 100m:N2}";
        }

        public void HideTrayIcon()
        {
            _icon.Visible = false;
        }

        public void ConstructContextMenu(List<string> currencies)
        {
            if (_contextMenu.InvokeRequired)
            {
                var safeCallDelegate = new SafeCallDelegate(ConstructContextMenu);

                _contextMenu.Invoke(safeCallDelegate, currencies);
            }
            else
            {
                var allCurrenciesChecked = _allCurrencies?.Checked ?? false;

                var isChecked = new List<string>();

                foreach (var item in _contextMenu.Items)
                {
                    if (item is ToolStripMenuItem menuItem && menuItem.Tag != null && menuItem.Checked)
                    {
                        isChecked.Add((string) menuItem.Tag);
                    }
                }

                _contextMenu.Items.Clear();

                _allCurrencies = new ToolStripMenuItem("Total", null, (_, _) => ToggleCurrencyHistory()) { Checked = allCurrenciesChecked };

                _contextMenu.Items.Add(_allCurrencies);

                if (currencies != null && currencies.Count > 1)
                {
                    foreach (var currency in currencies.OrderBy(c => c).ToList())
                    {
                        _contextMenu.Items.Add(new ToolStripMenuItem(currency.ToUpperInvariant(), null, (_, _) => ToggleCurrencyHistory(currency)) { Tag = currency, Checked = isChecked.Contains(currency) });
                    }
                }

                _contextMenu.Items.Add(new ToolStripSeparator());

                _alwaysOnTop = new ToolStripMenuItem("Keep Windows Above Others", null, (_, _) => ToggleTopMost()) { Checked = AppSettings.Instance.AlwaysOnTop };

                _contextMenu.Items.Add(_alwaysOnTop);

                _contextMenu.Items.Add(new ToolStripSeparator());

                _contextMenu.Items.Add(new ToolStripMenuItem("Refresh", null, (_, _) => RefreshClicked()));

                _contextMenu.Items.Add(new ToolStripSeparator());

                _contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitClicked()));
            }
        }

        private void TrayIconClicked(object sender, EventArgs eventArgs)
        {
            if (((MouseEventArgs) eventArgs).Button == MouseButtons.Left)
            {
                IconClicked();
            }
        }

        private void ToggleCurrencyHistory(string currency = null)
        {
            bool isChecked;

            if (string.IsNullOrWhiteSpace(currency))
            {
                _allCurrencies.Checked = ! _allCurrencies.Checked;

                isChecked = _allCurrencies.Checked;
            }
            else
            {
                var item = _contextMenu.Items.GetItem(currency);

                item.Checked = ! item.Checked;
                
                isChecked = item.Checked;
            }

            if (isChecked)
            {
                ShowCurrencyHistoryClicked(currency);
            }
            else
            {
                HideCurrencyHistoryClicked(currency);
            }
        }

        private string Difference(int balance)
        {
            if (_previousBalance == 0)
            {
                _previousBalance = balance;

                return string.Empty;
            }

            var difference = balance - _previousBalance;

            _previousBalance = balance;

            return $" {(difference < 0 ? string.Empty : '+')}£{difference / 100m:N2}";
        }

        private void ToggleTopMost()
        {
            AppSettings.Instance.AlwaysOnTop = ! AppSettings.Instance.AlwaysOnTop;

            AppSettings.Instance.Save();

            _alwaysOnTop.Checked = AppSettings.Instance.AlwaysOnTop;

            TopMostToggled(AppSettings.Instance.AlwaysOnTop);
        }
    }
}