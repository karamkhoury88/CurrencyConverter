using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    public record CurrencyConverterThirdPartyApiConfigurationDto
    {
        [JsonPropertyName(name: "BaseUrl")]
        [Required]
        public required string BaseUrl { get; set; }

        [JsonPropertyName(name: "LatestRatesCacheLifeTime")]
        public int LatestRatesCacheLifeTime { get; set; } = 10;

        [JsonPropertyName(name: "HistoricalRatesCacheLifeTime")]
        public int HistoricalRatesCacheLifeTime { get; set; } = 120;
        [JsonPropertyName(name: "AllowedCurrencyCodes")]
        public HashSet<string> AllowedCurrencyCodes { get; set; } = [];

        public bool IsCurrencyBanned(string currency) => !AllowedCurrencyCodes.Contains(currency, StringComparer.OrdinalIgnoreCase);
    }
}
