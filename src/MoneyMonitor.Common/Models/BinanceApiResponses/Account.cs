using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.BinanceApiResponses
{
    public class Account
    {
        [JsonPropertyName("balances")]
        public Balance[] Balances { get; set; }
    }
}