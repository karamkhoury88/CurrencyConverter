using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the IP-specific rate limit settings.
    /// </summary>
    public class IpRateLimitConfigurationDto
    {
        /// <summary>
        /// Gets or sets the maximum number of requests allowed from an IP address within the specified window.
        /// </summary>
        [JsonPropertyName("PermitLimit")]
        public int PermitLimit { get; set; } = 100;

        /// <summary>
        /// Gets or sets the time window (in minutes) for the rate limiting.
        /// </summary>
        [JsonPropertyName("Window")]
        public int Window { get; set; } = 1;
    }


}
