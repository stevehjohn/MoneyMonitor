using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseApiResponses
{
    public class Pagination
    {
        [JsonPropertyName("next_uri")]
        public string NextUri { get; set; }
    }
}