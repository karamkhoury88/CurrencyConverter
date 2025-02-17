using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the configuration settings for a third-party currency exchange API.
    /// </summary>
    public record CurrencyConverterThirdPartyApiConfigurationDto
    {
        /// <summary>
        /// The base URL of the third party currency exchange API
        /// </summary>
        [JsonPropertyName(name: "BaseUrl")]
        [Required]
        public required string BaseUrl { get; set; }

        /// <summary>
        /// The time in minutes of keeping the cached records of latest rates
        /// </summary>
        [JsonPropertyName(name: "LatestRatesCacheLifeTime")]
        public int LatestRatesCacheLifeTime { get; set; } = 10;

        /// <summary>
        /// The time in minutes of keeping the cached records of historical rates
        /// </summary>
        [JsonPropertyName(name: "HistoricalRatesCacheLifeTime")]
        public int HistoricalRatesCacheLifeTime { get; set; } = 120;

        /// <summary>
        /// A collection of the supported currencies 
        /// </summary>
        [JsonPropertyName(name: "AllowedCurrencyCodes")]
        public HashSet<string> AllowedCurrencyCodes { get; set; } = [];

        /// <summary>
        /// Check if the currency is banned (not supported)
        /// </summary>
        /// <param name="currency"></param>
        /// <returns></returns>
        public bool IsCurrencyBanned(string currency) => !AllowedCurrencyCodes.Contains(currency, StringComparer.OrdinalIgnoreCase);
    }
}
