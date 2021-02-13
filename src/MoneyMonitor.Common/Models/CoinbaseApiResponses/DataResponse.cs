using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseApiResponses
{
    public class DataResponse<T>
    {
        [JsonPropertyName("data")]
        public T Data { get; set; }
    }
}