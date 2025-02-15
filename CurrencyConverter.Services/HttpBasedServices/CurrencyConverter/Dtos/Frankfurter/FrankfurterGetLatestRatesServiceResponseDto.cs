using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos.Frankfurter
{
    public record FrankfurterGetLatestRatesServiceResponseDto
    {
        [JsonPropertyName("base")]
        [Required]
        public required string Base { get; init; }    // The base currency (e.g., "EUR")

        [JsonPropertyName("date")]
        [Required]
        public DateTime Date { get; init; }  // The date of the exchange rates

        [JsonPropertyName("rates")]
        [Required]
        public Dictionary<string, decimal> Rates { get; init; } = []; 
    }
}
