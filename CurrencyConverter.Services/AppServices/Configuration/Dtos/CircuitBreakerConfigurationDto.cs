using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the configuration settings for API circuit breaker pattern to handle and manage request failures.
    /// </summary>
    public record CircuitBreakerConfigurationDto : BaseConfigurationDto
    {
        /// <summary>
        /// Threshold for the number of failures before opening the circuit.
        /// </summary>
        [JsonPropertyName("FailureThreshold")]
        public int FailureThreshold { get; set; } = 100;

        /// <summary>
        /// Duration for which the circuit remains open before transitioning to half-open.
        /// </summary>
        [JsonPropertyName("CircuitOpenDuration")]
        public int CircuitOpenDuration { get; set; } = 5;

        /// <summary>
        /// Duration for which the circuit remains half-open before transitioning to closed.
        /// </summary>
        [JsonPropertyName("HalfOpenDuration")]
        public int HalfOpenDuration { get; set; } = 2;
    }
}
