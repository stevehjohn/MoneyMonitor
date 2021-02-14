using System;

namespace MoneyMonitor.Windows.Exceptions
{
    public class MoneyMonitorConfigurationException : Exception
    {
        public MoneyMonitorConfigurationException(string message) : base(message)
        {
        }
    }
}