using CurrencyConverter.ServiceDefaults.Constants;
using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Factories;
using Moq;
using Microsoft.Extensions.DependencyInjection;


namespace CurrencyConverter.Tests.UnitTests
{
    public class CurrencyConverterFactoryTests
    {
        [Fact]
        public void GetConverter_ReturnsFrankfurterCurrencyConverterService_ForFrankfurterProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var mockFrankfurterService = new Mock<ICurrencyConverterService>(); // Mock the interface

            // Mock GetService to return the Frankfurter service
            mockServiceProvider
                .Setup(x => x.GetService(typeof(ICurrencyConverterService)))
                .Returns(mockFrankfurterService.Object);

            var factory = new CurrencyConverterFactory(mockServiceProvider.Object);

            // Act
            var result = factory.GetConverter(CurrencyConverterProviders.FRANKFURTER);

            // Assert
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ICurrencyConverterService>(result); // Ensure the result implements the interface
        }

        [Fact]
        public void GetConverter_ThrowsAppException_ForUnsupportedProvider()
        {
            // Arrange
            var mockServiceProvider = new Mock<IServiceProvider>();
            var factory = new CurrencyConverterFactory(mockServiceProvider.Object);

            string unsupportedProvider = "UnsupportedProvider";

            // Act & Assert
            var exception = Assert.Throws<AppException>(() => factory.GetConverter(unsupportedProvider));

            Assert.Equal(AppErrorCode.NOT_ALLOWED_OPERATION, exception.ErrorCode);
            Assert.Contains($"The provider {unsupportedProvider} is not supported", exception.NonTechnicalMessage);
            Assert.Contains($"Provider {unsupportedProvider} is not supported.", exception.TechnicalMessage);
        }
    }
}
