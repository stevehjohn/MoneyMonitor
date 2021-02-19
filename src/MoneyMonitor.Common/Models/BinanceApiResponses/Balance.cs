using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.BinanceApiResponses
{
    public class Balance
    {
        [JsonPropertyName("asset")]
        public string Asset { get; set; }

        [JsonPropertyName("free")]
        public decimal Free { get; set; }
    }
}