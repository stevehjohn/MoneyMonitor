using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseApiResponses
{
    public class Account
    {
        [JsonPropertyName("balance")]
        public Balance Balance { get; set; }
    }
}