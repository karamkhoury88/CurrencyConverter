using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.Configuration.Dtos
{
    public record PollyResiliencePolicyConfigurationDto: BaseConfigurationDto
    {
        [JsonPropertyName("DurationOfBreak")]
        public int DurationOfBreak { get; set; } = 30;

        [JsonPropertyName("MaxRetryCount")]
        public int MaxRetryCount { get; set; } = 5;
    }
}
