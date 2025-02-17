using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the connection strings configuration for the Currency Converter application.
    /// </summary>
    public record ConnectionStringsConfigurationDto : BaseConfigurationDto
    {
        /// <summary>
        /// The database connection string.
        /// This property can be null if the connection string is not provided.
        /// </summary>
        [JsonPropertyName("DbConnection")]
        public string? DbConnection { get; set; }
    }
}
