using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.Configuration.Dtos
{
    public record RateLimitingConfigurationDto : BaseConfigurationDto
    {
        [JsonPropertyName("PermitLimit")]
        public int PermitLimit { get; set; } = 10;

        [JsonPropertyName("Window")]
        public int Window { get; set; } = 1;
    }
}
