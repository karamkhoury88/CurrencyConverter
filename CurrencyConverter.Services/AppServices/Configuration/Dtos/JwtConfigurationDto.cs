using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents the configuration settings for JWT (JSON Web Token).
    /// </summary>
    public record JwtConfigurationDto : BaseConfigurationDto
    {
        /// <summary>
        ///  The secret key used for signing the JWT.
        /// </summary>
        [JsonPropertyName("SecretKey")]
        [Required]
        public string SecretKey { get; set; }

        /// <summary>
        ///  The issuer of the JWT.
        /// </summary>
        [JsonPropertyName("Issuer")]
        [Required]
        public string Issuer { get; set; }

        /// <summary>
        ///  The audience of the JWT.
        /// </summary>
        [JsonPropertyName("Audience")]
        [Required]
        public string Audience { get; set; }

        /// <summary>
        ///  The number of days until the JWT expires.
        /// </summary>
        [JsonPropertyName("ExpireDays")]
        [Required]
        public int ExpireDays { get; set; }
    }


}
