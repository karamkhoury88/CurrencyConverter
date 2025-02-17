using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the configuration settings for the Currency Converter application.
    /// </summary>
    public record CurrencyConverterConfigurationDto : BaseConfigurationDto
    {
        /// <summary>
        /// The connection strings for any remote resource e.g. databases
        /// </summary>
        [JsonPropertyName("ConnectionStrings")]
        public ConnectionStringsConfigurationDto ConnectionStrings { get; set; }

        /// <summary>
        /// The JWT configuration to control the issuing of authurization tokens
        /// </summary>
        [JsonPropertyName("Jwt")]
        [Required]
        public JwtConfigurationDto Jwt { get; set; }
        
        /// <summary>
        /// Currency conversion and rate exchange provider settings
        /// </summary>
        [JsonPropertyName("CurrencyConverterThirdPartyApi")]
        [Required]
        public CurrencyConverterThirdPartyApiConfigurationDto CurrencyConverterThirdPartyApi { get; set; }

        /// <summary>
        /// The settings for API circuit breaker pattern to handle and manage request failures.
        /// </summary>
        [JsonPropertyName("CircuitBreaker")]
        [Required]
        public CircuitBreakerConfigurationDto CircuitBreaker { get; set; }
    }
}
