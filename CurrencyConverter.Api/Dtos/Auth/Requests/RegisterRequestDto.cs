using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Api.Dtos.Auth.Requests
{
    /// <summary>
    /// Represents the data transfer object for a user registration request.
    /// </summary>
    public record RegisterRequestDto
    {
        /// <summary>
        /// The email address of the user to be registered.
        /// </summary>
        [Required]
        [EmailAddress]
        public required string Email { get; init; }

        /// <summary>
        /// The password for the user to be registered.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; init; }
    }
}