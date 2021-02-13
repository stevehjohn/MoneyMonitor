using System;
using System.Windows.Forms;
using MoneyMonitor.Windows.Infrastructure.Settings;
using MoneyMonitor.Windows.Resources;

namespace MoneyMonitor.Windows.Services
{
    public class TrayManager
    {
        private readonly NotifyIcon _icon;

        private int _previousBalance;

        private readonly ContextMenuStrip _contextMenu;

        public Action ExitClicked { set; private get; }

        public TrayManager()
        {
            _contextMenu = new ContextMenuStrip();

            _icon = new NotifyIcon
                    {
                        ContextMenuStrip = _contextMenu,
                        Icon = Icons.right,
                        Visible = true
                    };

            ConstructContextMenu();
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

        private void ConstructContextMenu()
        {
            _contextMenu.Items.Clear();

            //_contextMenu.Items.Add(new ToolStripMenuItem("Float History Window", null, (_, _) => ToggleFloatHistory()) { Checked = AppSettings.Instance.FloatHistory });

            //_contextMenu.Items.Add(new ToolStripSeparator());

            _contextMenu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitClicked()));
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

            return $" {(difference < 0 ? string.Empty : '+')}{difference / 100m:N2}";
        }
    }
}