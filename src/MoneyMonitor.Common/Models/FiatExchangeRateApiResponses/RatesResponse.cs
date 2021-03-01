using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MoneyMonitor.Common.Models.FiatExchangeRateApiResponses
{
    public class RatesResponse
    {
        [JsonPropertyName("rates")] 
        public Dictionary<string, decimal> Rates { get; set; }
    }
}