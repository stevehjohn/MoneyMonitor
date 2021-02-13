using System;
using System.Windows.Forms;
using MoneyMonitor.Windows.Controls;

namespace MoneyMonitor.Windows.Forms
{
    public partial class History : Form
    {
        public HistoryChart HistoryChart { get; set; }

        public History()
        {
            InitializeComponent();

            HistoryChart = new HistoryChart
                           {
                               Left = 1,
                               Top = 1,
                               Width = Width - 2,
                               Height = Height - 2
                           };

            Controls.Add(HistoryChart);

            Deactivate += OnDeactivate;
        }

        private void OnDeactivate(object sender, EventArgs e)
        {
            if (! TopMost)
            {
                Close();
            }
        }
    }
}
