using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MoneyMonitor.Common.Services;
using MoneyMonitor.Windows.Forms;
using MoneyMonitor.Windows.Infrastructure;
using MoneyMonitor.Windows.Infrastructure.Settings;

namespace MoneyMonitor.Windows.Services
{
    public class FormManager
    {
        private readonly HistoryManager _historyManager;

        private readonly List<History> _forms;

        private readonly Color[] _colours;

        private int _colourIndex;

        public FormManager(HistoryManager historyManager)
        {
            _historyManager = historyManager;

            _forms = new List<History>();

            _colours = new[] { Color.Coral, Color.DeepPink, Color.DarkSeaGreen, Color.DodgerBlue, Color.Gold, Color.Lime, Color.DarkTurquoise, Color.LightBlue, Color.MediumSeaGreen, Color.Red };
        }

        public void ShowHistory(bool transient, string currency = null)
        {
            var form = new History
                       {
                           Currency = currency,
                           Width = Constants.HistoryWidth,
                           Height = Constants.HistoryHeight,
                           Left = Screen.PrimaryScreen.WorkingArea.Width - Constants.HistoryWidth,
                           Top = Screen.PrimaryScreen.WorkingArea.Height - Constants.HistoryHeight,
                           HistoryChart =
                           {
                               Title = currency?.ToUpperInvariant() ?? "All Currencies",
                               CurrencySymbol = AppSettings.Instance.FiatCurrencySymbol,
                               ExchangeRate = currency == null
                                   ? null
                                   : _historyManager.GetExchangeRate(currency)
                           }
                       };

            form.Closed += FormOnClosed;

            _forms.Add(form);

            form.Show(transient);

            form.Activate();

            if (! transient)
            {
                form.TopMost = AppSettings.Instance.AlwaysOnTop;

                if (! string.IsNullOrWhiteSpace(currency))
                {
                    form.HistoryChart.BarColour = _colours[_colourIndex];

                    _colourIndex++;

                    if (_colourIndex >= _colours.Length)
                    {
                        _colourIndex = 0;
                    }
                }
            }

            form.HistoryChart.UpdateData(_historyManager.GetHistory(currency));
        }

        public void CloseForm(string currency)
        {
            // ReSharper disable once PossibleNullReferenceException
            _forms.FirstOrDefault(f => (f.Currency ?? string.Empty).Equals(currency ?? string.Empty, StringComparison.InvariantCultureIgnoreCase)).Close();
        }

        public void NewData()
        {
            foreach (var form in _forms)
            {
                form.HistoryChart.UpdateData(_historyManager.GetHistory(form.Currency));
            }
        }

        public void UpdateTopMost(bool topMost)
        {
            foreach (var form in _forms)
            {
                if (! form.IsTransient)
                {
                    form.TopMost = topMost;
                }
            }
        }

        private void FormOnClosed(object sender, EventArgs e)
        {
            _forms.Remove((History) sender);
        }
    }
}