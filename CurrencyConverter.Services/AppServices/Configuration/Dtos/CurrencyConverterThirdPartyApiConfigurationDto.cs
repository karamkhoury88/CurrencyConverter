using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    public record CurrencyConverterThirdPartyApiConfigurationDto
    {
        [JsonPropertyName(name: "BaseUrl")]
        [Required]
        public string BaseUrl { get; set; }

        [JsonPropertyName(name: "LatestRatesCacheLifeTime")]
        public int LatestRatesCacheLifeTime { get; set; } = 60;
    }
}
