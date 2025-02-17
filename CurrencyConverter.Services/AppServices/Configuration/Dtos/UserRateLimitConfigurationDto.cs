using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the user-specific rate limit settings.
    /// </summary>
    public class UserRateLimitConfigurationDto
    {
        /// <summary>
        /// Gets or sets the maximum number of requests allowed for a user within the specified window.
        /// </summary>
        [JsonPropertyName("PermitLimit")]
        public int PermitLimit { get; set; } = 200;

        /// <summary>
        /// Gets or sets the time window (in minutes) for the rate limiting.
        /// </summary>
        [JsonPropertyName("Window")]
        public int Window { get; set; } = 1;
    }


}
