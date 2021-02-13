using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.CoinbaseApiResponses
{
    public class RatesDictionary
    {
        [JsonPropertyName("rates")]
        public Dictionary<string, string> Rates { get; set; }
    }
}