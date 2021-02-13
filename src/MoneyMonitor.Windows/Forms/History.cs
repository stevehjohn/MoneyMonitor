using System.Windows.Forms;
using MoneyMonitor.Windows.Controls;

namespace MoneyMonitor.Windows.Forms
{
    public partial class History : Form
    {
        public History()
        {
            InitializeComponent();

            Controls.Add(new HistoryChart
                         {
                             Left = 1,
                             Top = 1,
                             Width = Width - 2,
                             Height = Height - 2
                         });
        }
    }
}
