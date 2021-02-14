using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseProApiResponses
{
    public class Ticker
    {
        [JsonPropertyName("price")]
        public string Price { get; set; }
    }
}