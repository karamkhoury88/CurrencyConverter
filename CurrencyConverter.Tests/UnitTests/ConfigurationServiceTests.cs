using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.AppServices.Configuration.Dtos;
using Microsoft.Extensions.Configuration;
using Moq;

namespace CurrencyConverter.Tests.UnitTests
{
    public class ConfigurationServiceTests
    {
        [Fact]
        public void Constructor_WithValidConfiguration_SetsConfigProperty()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"CurrencyConverterConfiguration:ConnectionStrings:DbConnection", "Server=myServer;Database=myDb;"},

                {"CurrencyConverterConfiguration:Jwt:SecretKey", "mySecretKey"},
                {"CurrencyConverterConfiguration:Jwt:Issuer", "myIssuer"},
                {"CurrencyConverterConfiguration:Jwt:Audience", "myAudience"},
                {"CurrencyConverterConfiguration:Jwt:ExpireDays", "7"},

                {"CurrencyConverterConfiguration:CurrencyConverterThirdPartyApi:BaseUrl", "https://api.currencyconverter.com"},
                {"CurrencyConverterConfiguration:CurrencyConverterThirdPartyApi:LatestRatesCacheLifeTime", "10"},
                {"CurrencyConverterConfiguration:CurrencyConverterThirdPartyApi:HistoricalRatesCacheLifeTime", "120"},
                {"CurrencyConverterConfiguration:CurrencyConverterThirdPartyApi:AllowedCurrencyCodes:0", "USD"},
                {"CurrencyConverterConfiguration:CurrencyConverterThirdPartyApi:AllowedCurrencyCodes:1", "EUR"},

                {"CurrencyConverterConfiguration:CircuitBreaker:FailureThreshold", "50"},
                {"CurrencyConverterConfiguration:CircuitBreaker:CircuitOpenDuration", "10"},
                {"CurrencyConverterConfiguration:CircuitBreaker:HalfOpenDuration", "5"},

                {"CurrencyConverterConfiguration:RateLimiting:User:PermitLimit", "200"},
                {"CurrencyConverterConfiguration:RateLimiting:User:Window", "1"},
                {"CurrencyConverterConfiguration:RateLimiting:Ip:PermitLimit", "100"},
                {"CurrencyConverterConfiguration:RateLimiting:Ip:Window", "2"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Act
            var configurationService = new ConfigurationService(configuration);

            // Assert
            Assert.NotNull(configurationService.Config);
           
            Assert.NotNull(configurationService.Config.ConnectionStrings);
            
            Assert.Equal("Server=myServer;Database=myDb;", configurationService.Config.ConnectionStrings.DbConnection);
            Assert.NotNull(configurationService.Config.Jwt);
            Assert.Equal("mySecretKey", configurationService.Config.Jwt.SecretKey);
            Assert.Equal("myIssuer", configurationService.Config.Jwt.Issuer);
            Assert.Equal("myAudience", configurationService.Config.Jwt.Audience);
            Assert.Equal(7, configurationService.Config.Jwt.ExpireDays);
           
            Assert.NotNull(configurationService.Config.CurrencyConverterThirdPartyApi);
            Assert.Equal("https://api.currencyconverter.com", configurationService.Config.CurrencyConverterThirdPartyApi.BaseUrl);
            Assert.Equal(10, configurationService.Config.CurrencyConverterThirdPartyApi.LatestRatesCacheLifeTime);
            Assert.Equal(120, configurationService.Config.CurrencyConverterThirdPartyApi.HistoricalRatesCacheLifeTime);
            Assert.Contains("USD", configurationService.Config.CurrencyConverterThirdPartyApi.AllowedCurrencyCodes);
            Assert.Contains("EUR", configurationService.Config.CurrencyConverterThirdPartyApi.AllowedCurrencyCodes);           

            Assert.Equal(50, configurationService.Config.CircuitBreaker.FailureThreshold);
            Assert.Equal(10, configurationService.Config.CircuitBreaker.CircuitOpenDuration);
            Assert.Equal(5, configurationService.Config.CircuitBreaker.HalfOpenDuration);

            Assert.Equal(200, configurationService.Config.RateLimiting.User.PermitLimit);
            Assert.Equal(1, configurationService.Config.RateLimiting.User.Window);
            Assert.Equal(100, configurationService.Config.RateLimiting.Ip.PermitLimit);
            Assert.Equal(2, configurationService.Config.RateLimiting.Ip.Window);
        }

        [Fact]
        public void Constructor_WithMissingConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            IConfiguration configuration = new ConfigurationBuilder().Build();

            // Act & Assert
            _ = Assert.Throws<InvalidOperationException>(() => new ConfigurationService(configuration));
        }

        [Fact]
        public void Constructor_WithInvalidConfiguration_ThrowsInvalidOperationException()
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"CurrencyConverterConfiguration:ConnectionStrings:DbConnection", "Server=myServer;Database=myDb;"},
                // Missing Jwt and CurrencyConverterThirdPartyApi
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => new ConfigurationService(configuration));
            Assert.Contains("Configuration is invalid. Errors:", exception.Message);
        }
    }
}
