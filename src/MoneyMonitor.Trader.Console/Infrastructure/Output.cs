using System;
using System.IO;

namespace MoneyMonitor.Trader.Console.Infrastructure
{
    public class Output
    {
        private readonly string _fileName;

        private bool _previousSameLine;

        public Output(string fileName)
        {
            _fileName = fileName;
        }

        public void Write(string output, ConsoleColor color, bool sameLine = false)
        {
            System.Console.ForegroundColor = color;

            if (sameLine)
            {
                if (_previousSameLine)
                {
                    System.Console.CursorTop -= 1;
                }
                else
                {
                    _previousSameLine = true;
                }
            }
            else
            {
                _previousSameLine = false;
            }

            System.Console.WriteLine($"{output}          ");

            File.AppendAllLines(_fileName, new [] { output });
        }
    }
}