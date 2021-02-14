using System;
using System.Windows.Forms;
using OfficeOpenXml;

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

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            Application.Run(new Context());
        }
    }
}