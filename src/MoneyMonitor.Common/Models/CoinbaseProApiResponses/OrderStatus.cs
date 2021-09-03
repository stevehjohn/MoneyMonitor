using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseProApiResponses
{
    public class OrderStatus
    {
        [JsonPropertyName("status")]
        public string Status { get; set; }
    }
}