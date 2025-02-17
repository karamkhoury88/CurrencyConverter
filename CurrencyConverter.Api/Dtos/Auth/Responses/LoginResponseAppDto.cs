using CurrencyConverter.Api.Dtos.Bases.Responses;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.Auth.Responses
{
    /// <summary>
    /// Represents the data transfer object for a login response, containing the JWT token.
    /// </summary>
    public record LoginResponseAppDto : BaseResponseAppDTO
    {
        /// <summary>
        /// The JWT token generated for the authenticated user.
        /// </summary>
        [JsonPropertyName("token")]
        [Required]
        public required string Token { get; init; }
    }
}