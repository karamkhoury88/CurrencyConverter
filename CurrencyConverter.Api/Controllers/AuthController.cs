using Asp.Versioning;
using CurrencyConverter.Api.Common;
using CurrencyConverter.Api.Common.Exceptions;
using CurrencyConverter.Api.Dtos.Auth.Requests;
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
    [ApiVersion("1.0")]
    public class AuthController(UserManager<CurrencyConverterUser> userManager,
        SignInManager<CurrencyConverterUser> signInManager,
        IConfigurationService configuration,
        ILogger<AuthController> logger) : AppBaseController
    {
        /// <summary>
        /// Register a new user to the system
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("client/register")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            var user = new CurrencyConverterUser { UserName = model.Email, Email = model.Email };
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, CurrencyConverterAuthorizationRole.USER);
                return Ok(new { message = "User registered successfully" });
            }

            throw new AppException(errorCode: AppErrorCode.INVALID_PARAMETER,
                publicMessage: string.Join(", ", result.Errors.Select(x => x.Description)));
        }


        /// <summary>
        /// Login to the system
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost("login")]
        [MapToApiVersion("1.0")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                CurrencyConverterUser? user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && !string.IsNullOrWhiteSpace(user.Email) && !string.IsNullOrWhiteSpace(user.UserName))
                {
                    var roles = await userManager.GetRolesAsync(user);
                    var token = GenerateJwtToken(user, roles);
                    return Ok(new { token });
                }
            }
            return Unauthorized();
        }

        #region Privates

        private string GenerateJwtToken(CurrencyConverterUser user, IList<string> roles)
        {
            List<Claim> claims = [
                new(ClaimTypes.NameIdentifier, user.Id),
                new (ClaimTypes.Email, user.Email!),
                new (ClaimTypes.Name, user.UserName!)
            ];

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.Config.Jwt.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddDays(Convert.ToDouble(configuration.Config.Jwt.ExpireDays));

            var token = new JwtSecurityToken(
                configuration.Config.Jwt.Issuer,
                configuration.Config.Jwt.Audience,
                claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion
    }
}
