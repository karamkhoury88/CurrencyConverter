using Asp.Versioning;
using CurrencyConverter.Api.Common;
using CurrencyConverter.Api.Dtos.Auth.Requests;
using CurrencyConverter.Api.Dtos.Auth.Responses;
using CurrencyConverter.Data.Models;
using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.Services.AppServices.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CurrencyConverter.Api.Controllers
{
    /// <summary>
    /// Controller for handling authentication-related operations such as user registration and login.
    /// </summary>
    [ApiVersion("1.0")]
    public class AuthController(UserManager<CurrencyConverterUser> userManager,
        SignInManager<CurrencyConverterUser> signInManager,
        IConfigurationService configuration,
        ILogger<AuthController> logger) : AppBaseController
    {
        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="model">The registration request data.</param>
        /// <returns>A response indicating success or failure of the registration process.</returns>
        [HttpPost("client/register")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            // Create a new user with the provided email and password.
            var user = new CurrencyConverterUser { UserName = model.Email, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign the USER role to the newly created user.
                await userManager.AddToRoleAsync(user, CurrencyConverterAuthorizationRole.USER);
                return Ok(new { message = "User registered successfully" });
            }

            // Throw an exception if user creation fails, including error details.
            throw new AppException(errorCode: AppErrorCode.INVALID_PARAMETER,
                nonTechnicalMessage: string.Join(", ", result.Errors.Select(x => x.Description)),
                technicalMessage: "Register user model is not valid."
                );
        }

        /// <summary>
        /// Authenticates a user and generates a JWT token for login.
        /// </summary>
        /// <param name="model">The login request data.</param>
        /// <returns>A response containing the JWT token if login is successful; otherwise, an unauthorized response.</returns>
        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(LoginResponseAppDto), 200)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            // Attempt to sign in the user with the provided credentials.
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                // Retrieve the user and their roles if login is successful.
                CurrencyConverterUser? user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(user.UserName))
                {
                    var roles = await userManager.GetRolesAsync(user);
                    // Generate a JWT token for the authenticated user.
                    var token = GenerateJwtToken(user, roles);
                    return Ok(new LoginResponseAppDto { Token = token });
                }
            }
            // Return an unauthorized response if login fails.
            return Unauthorized();
        }

        #region Privates

        /// <summary>
        /// Generates a JWT token for the authenticated user.
        /// </summary>
        /// <param name="user">The user for whom the token is generated.</param>
        /// <param name="roles">The roles assigned to the user.</param>
        /// <returns>A JWT token as a string.</returns>
        private string GenerateJwtToken(CurrencyConverterUser user, IList<string> roles)
        {
            // Create a list of claims for the token, including user ID, email, and roles.
            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, user.Id),
                new (ClaimTypes.Email, user.Email!),
                new (ClaimTypes.Name, user.UserName!)
            ];

            // Add role claims to the token.
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create a symmetric security key using the JWT secret key from configuration.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Config.Jwt.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            // Set the token expiration date based on the configuration.
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration.Config.Jwt.ExpireDays));

            // Create the JWT token with the specified claims, issuer, audience, and expiration.
            var token = new JwtSecurityToken(
                configuration.Config.Jwt.Issuer,
                configuration.Config.Jwt.Audience,
                claims,
                expires: expires,
                signingCredentials: credentials
            );

            // Serialize the token to a string and return it.
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion
    }
}