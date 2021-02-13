using System.Drawing;
using System.Windows.Forms;

namespace MoneyMonitor.Windows.Controls
{
    public sealed partial class HistoryChart : Control
    {
        public HistoryChart()
        {
            InitializeComponent();

            DoubleBuffered = true;

            SetStyle(ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.Clear(Color.Black);
        }
    }
}
