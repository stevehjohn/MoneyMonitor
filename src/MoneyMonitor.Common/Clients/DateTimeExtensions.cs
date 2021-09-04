using System;
using System.Globalization;

namespace MoneyMonitor.Common.Clients
{
    public static class DateTimeExtensions
    {
        public static string ToISO8601(this DateTime datetime)
        {
            return datetime.ToString("s", CultureInfo.InvariantCulture);
        }
    }
}