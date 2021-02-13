using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseApiResponses
{
    public class PaginatedResponse<T>
    {
        [JsonPropertyName("pagination")]
        public Pagination Pagination { get; set; }

        [JsonPropertyName("data")]
        public T[] Data { get; set; }
    }
}