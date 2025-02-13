using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.Configuration.Dtos
{
    public record CurrencyConverterConfigurationDto : BaseConfigurationDto
    {
        [JsonPropertyName("ConnectionStrings")]
        public ConnectionStringsConfigurationDto ConnectionStrings { get; set; }

        [JsonPropertyName("Jwt")]
        [Required]
        public JwtConfigurationDto Jwt { get; set; }

        [JsonPropertyName("PollyResiliencePolicy")]
        public PollyResiliencePolicyConfigurationDto PollyResiliencePolicy { get; set; } = new();

        [JsonPropertyName("AuthenticatedUserRateLimitingConfiguration")]
        public RateLimitingConfigurationDto AuthenticatedUserRateLimitingConfiguration { get; set; } = new();

        [JsonPropertyName("AnonymousUserRateLimitingConfiguration")]
        public RateLimitingConfigurationDto AnonymousUserRateLimitingConfiguration { get; set; } = new();
    }
}
