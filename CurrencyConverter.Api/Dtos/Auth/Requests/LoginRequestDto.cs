using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Api.Dtos.Auth.Requests
{
    /// <summary>
    /// Represents the data transfer object for a login request.
    /// </summary>
    public class LoginRequestDto
    {
        /// <summary>
        /// The email address of the user attempting to log in.
        /// </summary>
        [Required]
        [EmailAddress]
        public required string Email { get; init; }

        /// <summary>
        /// The password of the user attempting to log in.
        /// </summary>
        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; init; }
    }
}