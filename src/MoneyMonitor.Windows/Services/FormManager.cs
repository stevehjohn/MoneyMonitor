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

        public void ShowHistory()
        {
            var form = new History
                       {
                           Width = Constants.HistoryWidth,
                           Height = Constants.HistoryHeight,
                           Left = Screen.PrimaryScreen.WorkingArea.Width - Constants.HistoryWidth,
                           Top = Screen.PrimaryScreen.WorkingArea.Height - Constants.HistoryHeight,
                           HistoryChart =
                           {
                               Title = "All Currencies",
                               CurrencySymbol = AppSettings.Instance.FiatCurrencySymbol
                           }
                       };

            form.Closed += FormOnClosed;

            _forms.Add(form);

            form.Show();

            form.Activate();

            form.HistoryChart.UpdateData(_historyManager.GetHistory());
        }

        public void NewData()
        {
            foreach (var form in _forms)
            {
                form.HistoryChart.UpdateData(_historyManager.GetHistory());
            }
        }

        private void FormOnClosed(object sender, EventArgs e)
        {
            _forms.Remove((History) sender);
        }
    }
}