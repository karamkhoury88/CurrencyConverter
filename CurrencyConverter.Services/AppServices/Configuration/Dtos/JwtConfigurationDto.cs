using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    public record JwtConfigurationDto : BaseConfigurationDto
    {
        [JsonPropertyName("SecretKey")]
        [Required]
        public string SecretKey { get; set; }

        [JsonPropertyName("Issuer")]
        [Required]
        public string Issuer { get; set; }

        [JsonPropertyName("Audience")]
        [Required]
        public string Audience { get; set; }

        [JsonPropertyName("ExpireDays")]
        [Required]
        public int ExpireDays { get; set; }
    }
}
