using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseProApiRequests
{
    public class PlaceOrder
    {
        [JsonPropertyName("cancel_after")]
        public string CancelAfter { get; set; }

        [JsonPropertyName("product_id")]
        public string ProductId { get; set; }

        [JsonPropertyName("side")]
        public string Side { get; set; }

        [JsonPropertyName("stop")]
        public string Stop { get; set; }

        [JsonPropertyName("time_in_force")]
        public string TimeInForce { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }
}