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
                               Title = currency?.ToUpperInvariant() ?? "Total",
                               CurrencySymbol = AppSettings.Instance.FiatCurrencySymbol
                           },
                           Text = currency,
                           FormMoved = FormMoved
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

            form.HistoryChart.UpdateData(_historyManager.GetHistory(currency), _historyManager.GetHistoryTime(), _historyManager.GetExchangeRate(currency), _historyManager.GetHolding(currency));
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
                form.HistoryChart.UpdateData(_historyManager.GetHistory(form.Currency), _historyManager.GetHistoryTime(), _historyManager.GetExchangeRate(form.Currency), _historyManager.GetHolding(form.Currency));
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

        public void FormMoved(History form, Point position)
        {
            foreach (var item in _forms)
            {
                if (item == form)
                {
                    continue;
                }

                if (Math.Abs(form.Left + form.Width - item.Left) < 10 && Math.Abs(form.Top - item.Top) < 10)
                {
                    form.Left = item.Left - form.Width + 1;
                    form.Top = item.Top;

                    break;
                }

                if (Math.Abs(item.Left + item.Width - form.Left) < 10 && Math.Abs(form.Top - item.Top) < 10)
                {
                    form.Left = item.Left + item.Width - 2;
                    form.Top = item.Top;

                    break;
                }

                if (Math.Abs(item.Top + item.Height - form.Top) < 10 && Math.Abs(form.Left - item.Left) < 10)
                {
                    form.Left = item.Left;
                    form.Top = item.Top + item.Height - 1;

                    break;
                }

                if (Math.Abs(form.Top + form.Height - item.Top) < 10 && Math.Abs(form.Left - item.Left) < 10)
                {
                    form.Left = item.Left;
                    form.Top = item.Top - item.Height + 1;

                    break;
                }
            }
        }

        private void FormOnClosed(object sender, EventArgs e)
        {
            _forms.Remove((History) sender);
        }
    }
}