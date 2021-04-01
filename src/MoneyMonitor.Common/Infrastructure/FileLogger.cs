using System;
using System.Collections.Generic;
using System.IO;

namespace MoneyMonitor.Common.Infrastructure
{
    public class FileLogger : ILogger
    {
        private readonly string _filename;

        public FileLogger(string filename)
        {
            _filename = filename;
        }

        public void LogError(string message, Exception exception)
        {
            var lines = new List<string>
                        {
                            $"[{DateTime.UtcNow:s}] {message}" 
                        };

            var exceptionLines = exception.ToString().Split(Environment.NewLine);

            foreach (var line in exceptionLines)
            {
                lines.Add($"    {line}");
            }

            File.AppendAllLines(_filename, lines);
        }
    }
}