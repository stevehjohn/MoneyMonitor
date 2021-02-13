using System;
using System.Collections.Generic;
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

        public FormManager(HistoryManager historyManager)
        {
            _historyManager = historyManager;

            _forms = new List<History>();
        }

        public void ShowHistory(bool transient, string currency = null)
        {
            var form = new History
                       {
                           Width = Constants.HistoryWidth,
                           Height = Constants.HistoryHeight,
                           Left = Screen.PrimaryScreen.WorkingArea.Width - Constants.HistoryWidth,
                           Top = Screen.PrimaryScreen.WorkingArea.Height - Constants.HistoryHeight,
                           HistoryChart =
                           {
                               Title = currency?.ToUpperInvariant() ?? "All Currencies",
                               CurrencySymbol = AppSettings.Instance.FiatCurrencySymbol
                           }
                       };

            form.Closed += FormOnClosed;

            _forms.Add(form);

            form.Show(transient);

            form.Activate();

            form.HistoryChart.UpdateData(_historyManager.GetHistory(currency));
        }

        public void NewData()
        {
            foreach (var form in _forms)
            {
                form.HistoryChart.UpdateData(_historyManager.GetHistory());
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