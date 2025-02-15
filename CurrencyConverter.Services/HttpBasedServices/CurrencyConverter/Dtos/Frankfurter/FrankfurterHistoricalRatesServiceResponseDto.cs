using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos.Frankfurter
{
    public class FrankfurterHistoricalRatesServiceResponseDto
    {
        [JsonPropertyName("base")]
        [Required]
        public string Base { get; init; } // Base currency (e.g., "EUR")

        [JsonPropertyName("start_date")]
        [Required]
        public string StartDate { get; init; } // Start date of the historical data

        [JsonPropertyName("end_date")]
        [Required]
        public string EndDate { get; init; } // End date of the historical data

        [JsonPropertyName("rates")]
        [Required]
        public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; init; } = []; // Rates by date
    }
}
