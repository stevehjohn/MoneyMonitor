using System;
using System.Windows.Forms;

namespace MoneyMonitor.Windows.Infrastructure
{
    public static class StartUp
    {
        [STAThread]
        public static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new Context());
        }
    }
}