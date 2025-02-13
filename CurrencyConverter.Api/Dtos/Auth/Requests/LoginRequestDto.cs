using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Api.Dtos.Auth.Requests
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; init; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; init; }
    }
}
