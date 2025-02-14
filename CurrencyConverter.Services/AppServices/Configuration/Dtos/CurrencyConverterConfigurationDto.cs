using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    public record CurrencyConverterConfigurationDto : BaseConfigurationDto
    {
        [JsonPropertyName("ConnectionStrings")]
        public ConnectionStringsConfigurationDto ConnectionStrings { get; set; }

        [JsonPropertyName("Jwt")]
        [Required]
        public JwtConfigurationDto Jwt { get; set; }
        
        [JsonPropertyName("CurrencyConverterThirdPartyApi")]
        [Required]
        public CurrencyConverterThirdPartyApiConfigurationDto CurrencyConverterThirdPartyApi { get; set; }
    }
}
