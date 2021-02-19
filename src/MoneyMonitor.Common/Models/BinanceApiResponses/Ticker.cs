using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.BinanceApiResponses
{
    public class Ticker
    {
        [JsonPropertyName("price")]
        public string Price { get; set; }
    }
}