using System;

namespace MoneyMonitor.Common.Infrastructure
{
    public interface ILogger
    {
        void LogError(string message, Exception exception);
    }
}