using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using MoneyMonitor.Common.Services;
using MoneyMonitor.Windows.Forms;
using MoneyMonitor.Windows.Infrastructure;
using MoneyMonitor.Windows.Infrastructure.Settings;
using MoneyMonitor.Windows.Models;

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

            _colours = new[] { Color.Coral, Color.DeepPink, Color.DarkSeaGreen, Color.Aquamarine, Color.Gold, Color.Lime, Color.DarkTurquoise, Color.LightBlue, Color.MediumSeaGreen, Color.Red };
        }

        public void ShowHistory(bool transient, string currency = null, int? left = null, int? top = null)
        {
            var form = new History
                       {
                           Currency = currency,
                           Width = Constants.HistoryWidth,
                           Height = Constants.HistoryHeight,
                           Left = left ?? Screen.PrimaryScreen.WorkingArea.Width - Constants.HistoryWidth,
                           Top = top ?? Screen.PrimaryScreen.WorkingArea.Height - Constants.HistoryHeight,
                           HistoryChart =
                           {
                               Title = currency?.ToUpperInvariant() ?? "Total",
                               CurrencySymbol = AppSettings.Instance.FiatCurrencySymbol
                           },
                           Text = currency,
                           FormMoved = FormMoved,
                           CloseEventReceived = FormClosed
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

            form.HistoryChart.UpdateData(_historyManager.GetHistory(currency), LocaliseTime(_historyManager.GetHistoryTime()), _historyManager.GetExchangeRate(currency), _historyManager.GetHolding(currency), GetHoldingPercent(currency));
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
                form.HistoryChart.UpdateData(_historyManager.GetHistory(form.Currency), LocaliseTime(_historyManager.GetHistoryTime()), _historyManager.GetExchangeRate(form.Currency), _historyManager.GetHolding(form.Currency), GetHoldingPercent(form.Currency));
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
                    form.Snap(item.Left - form.Width + 1, item.Top);

                    break;
                }

                if (Math.Abs(item.Left + item.Width - form.Left) < 10 && Math.Abs(form.Top - item.Top) < 10)
                {
                    form.Snap(item.Left + item.Width - 2, item.Top);

                    break;
                }

                if (Math.Abs(item.Top + item.Height - form.Top) < 10 && Math.Abs(form.Left - item.Left) < 10)
                {
                    form.Snap(item.Left, item.Top + item.Height - 1);

                    break;
                }

                if (Math.Abs(form.Top + form.Height - item.Top) < 10 && Math.Abs(form.Left - item.Left) < 10)
                {
                    form.Snap(item.Left, item.Top - item.Height + 1);

                    break;
                }
            }
        }

        public void FormClosed()
        {
            SaveState();
        }

        public void SaveState()
        {
            var state = new List<FormState>();

            foreach (var form in _forms)
            {
                if (! form.IsTransient)
                {
                    state.Add(new FormState
                              {
                                  Currency = form.Currency,
                                  Left = form.Left,
                                  Top = form.Top
                              });
                }
            }

            File.WriteAllText(Constants.FormStateFilename, JsonSerializer.Serialize(state.ToArray()));
        }

        public void RestoreState()
        {
            if (! File.Exists(Constants.FormStateFilename))
            {
                return;
            }

            var stateJson = File.ReadAllText(Constants.FormStateFilename);

            var states = JsonSerializer.Deserialize<FormState[]>(stateJson);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var state in states)
            {
                ShowHistory(false, state.Currency, state.Left, state.Top);
            }
        }

        public bool IsFormShown(string currency)
        {
            return _forms.Any(f => f.Currency == currency);
        }

        private decimal? GetHoldingPercent(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
            {
                return null;
            }

            var totalValue = _historyManager.GetHistory().Last();

            var currencyValue = _historyManager.GetHistory(currency).Last();

            return (decimal) currencyValue / totalValue * 100;
        }

        private void FormOnClosed(object sender, EventArgs e)
        {
            _forms.Remove((History) sender);
        }

        private DateTime? LocaliseTime(DateTime? utc)
        {
            if (utc.HasValue)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(utc.Value, TimeZoneInfo.Local);
            }

            return null;
        }
    }
}