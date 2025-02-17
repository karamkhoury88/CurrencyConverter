using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the rate limiting settings for the application.
    /// </summary>
    public class RateLimitingConfigurationDto
    {
        /// <summary>
        /// Gets or sets the user-specific rate limiting settings.
        /// </summary>
        [JsonPropertyName("User")]
        [Required]
        public UserRateLimitConfigurationDto User { get; set; }

        /// <summary>
        /// Gets or sets the IP-specific rate limiting settings.
        /// </summary>
        [JsonPropertyName("Ip")]
        [Required]
        public IpRateLimitConfigurationDto Ip { get; set; }
    }


}
