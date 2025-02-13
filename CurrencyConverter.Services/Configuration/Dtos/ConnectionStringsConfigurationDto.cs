using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.Configuration.Dtos
{
    public record ConnectionStringsConfigurationDto : BaseConfigurationDto
    {
        [JsonPropertyName("DbConnection")]
        public string? DbConnection { get; set; }
    }
}
