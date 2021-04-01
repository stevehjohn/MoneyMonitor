using System;
using System.Windows.Forms;
using MoneyMonitor.Common.Infrastructure;
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

            try
            {
                Application.Run(new Context());
            }
            catch (Exception exception)
            {
                var logger = new FileLogger(Constants.LogFilename);

                logger.LogError("Fatal", exception);
            }
        }
    }
}