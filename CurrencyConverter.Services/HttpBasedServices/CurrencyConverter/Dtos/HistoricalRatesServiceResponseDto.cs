using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos
{
    public class HistoricalRatesServiceResponseDto
    {
        [JsonPropertyName("base")]
        [Required]
        public string Base { get; set; } // Base currency (e.g., "EUR")

        [JsonPropertyName("start_date")]
        [Required]
        public DateTime StartDate { get; set; } // Start date of the historical data

        [JsonPropertyName("end_date")]
        [Required]
        public DateTime EndDate { get; set; } // End date of the historical data

        [JsonPropertyName("rates")]
        [Required]
        public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; set; } = []; // Rates by date
    }
}
