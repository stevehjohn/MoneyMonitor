using System.IO;

namespace MoneyMonitor.Trader.Console.Infrastructure
{
    public class Output
    {
        private readonly string _fileName;

        public Output(string fileName)
        {
            _fileName = fileName;
        }

        public void Write(string output)
        {
            System.Console.WriteLine(output);

            File.AppendAllLines(_fileName, new [] { output });
        }
    }
}