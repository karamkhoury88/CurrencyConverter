using CurrencyConverter.Api.Common;
using CurrencyConverter.Api.Dtos.Auth.Requests;
using CurrencyConverter.Api.Dtos.Auth.Responses;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace CurrencyConverter.Tests.IntegrationUnitTests
{

    public class AuthControllerIntegrationTests : IClassFixture<IntegrationTestsFixture>
    {
        private readonly HttpClient _httpClient;

        // Base URIs
        private readonly string registerUri = "/api/v1/auth/client/register";
        private readonly string loginUri = "/api/v1/auth/login";
        private readonly LoginRequestDto _validLoginRequest = new()
        {
            Email = "user1@currencyconverter.com",
            Password = "User@123"
        };
        public AuthControllerIntegrationTests(IntegrationTestsFixture fixture)
        {
            _httpClient = fixture.HttpClient;
        }

        #region Registration

        [Fact]
        public async Task Register_ShouldReturnOk_WhenDataIsValid()
        {
            // Arrange
            var registerRequest = new RegisterRequestDto
            {
                Email = "newuser@newExample.com",
                Password = "ValidPassword123!"
            };
            var content = CreateJsonContent(registerRequest);

            // Act
            var response = await _httpClient.PostAsync(registerUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("User registered successfully", responseString);
        }

        /// <summary>
        /// Registering with an Existing Email
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            var content = CreateJsonContent(new RegisterRequestDto() {
                Email = _validLoginRequest.Email,
                Password = "Any!Password@123" 
            });

            // registration with the exisiting email
            var secondResponse = await _httpClient.PostAsync(registerUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        }

        /// <summary>
        ///  Registering with Invalid Email Format
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailFormatIsInvalid()
        {
            // Arrange
            var registerRequest = new RegisterRequestDto
            {
                Email = "invalid-email-format",
                Password = "ValidPassword123!"
            };
            var content = CreateJsonContent(registerRequest);

            // Act
            var response = await _httpClient.PostAsync(registerUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("The Email field is not a valid e-mail address.", responseString);
        }

        /// <summary>
        /// Registering with Weak Password
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenPasswordIsWeak()
        {
            // Arrange
            var registerRequest = new RegisterRequestDto
            {
                Email = "userwithweakpassword@example.com",
                Password = "12345"
            };
            var content = CreateJsonContent(registerRequest);

            // Act
            var response = await _httpClient.PostAsync(registerUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Passwords must be at least", responseString);
        }

        /// <summary>
        /// Registering Without Providing a Body
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenRequestBodyIsEmpty()
        {
            // Arrange
            var content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync(registerUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        /// <summary>
        /// SQL Injection or Malicious Input
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenEmailContainsSqlInjection()
        {
            // Arrange
            var maliciousEmail = "malicious@example.com'; DROP TABLE Users; --";
            var registerRequest = new RegisterRequestDto
            {
                Email = maliciousEmail,
                Password = "ValidPassword123!"
            };
            var content = CreateJsonContent(registerRequest);

            // Act
            var response = await _httpClient.PostAsync(registerUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Register_ShouldReturnMethodNotAllowed_WhenUsingGet()
        {
            // Arrange

            // Act
            var response = await _httpClient.GetAsync(registerUri);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion

        #region Login

        /// <summary>
        /// Logging in with Non-Existent User
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
        {
            // Arrange
            var loginRequest = new LoginRequestDto
            {
                Email = "nonexistentuser@example.com",
                Password = "SomePassword123!"
            };
            var content = CreateJsonContent(loginRequest);

            // Act
            var response = await _httpClient.PostAsync(loginUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturnOkWithToken_WhenCredentialsAreValid()
        {
            // Arrange
           
            var content = CreateJsonContent(_validLoginRequest);

            // Act
            var response = await _httpClient.PostAsync(loginUri, content);

            // Assert
            response.EnsureSuccessStatusCode(); // Ensure the response is 200 OK
            var responseString = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponseAppDto>(responseString);

            // Validate the response
            Assert.NotNull(loginResponse);
            Assert.False(string.IsNullOrWhiteSpace(loginResponse.Token));

            // Validate the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(loginResponse.Token);

            // Validate claims in the token
            var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            var expires = jwtToken.ValidTo;
            var expectedExpires = DateTime.UtcNow.AddDays(1); // Adjust based on your configuration

            Assert.Equal(_validLoginRequest.Email, emailClaim); // Validate email claim
            Assert.Equal(CurrencyConverterAuthorizationRole.USER, roleClaim); // Validate role claim
            Assert.True(expires > DateTime.UtcNow && expires <= expectedExpires);
        }

        /// <summary>
        ///  Logging in with an Incorrect Password
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsIncorrect()
        {
            // Arrange

            var incorrectPassword = "IncorrectPassword!";

            var loginRequest = new LoginRequestDto
            {
                Email = _validLoginRequest.Email,
                Password = incorrectPassword
            };
            var content = CreateJsonContent(loginRequest);

            // Act
            var response = await _httpClient.PostAsync(loginUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        /// <summary>
        /// Logging in Without Providing a Body
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenRequestBodyIsEmpty()
        {
            // Arrange
            var content = new StringContent("", Encoding.UTF8, "application/json");

            // Act
            var response = await _httpClient.PostAsync(loginUri, content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }



        /// <summary>
        /// Invalid HTTP Methods
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task Login_ShouldReturnMethodNotAllowed_WhenUsingGet()
        {
            // Arrange
            // Act
            var response = await _httpClient.GetAsync(loginUri);

            // Assert
            Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
        }

        #endregion


        private StringContent CreateJsonContent<T>(T data)
        {
            return new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
        }
    }
}
