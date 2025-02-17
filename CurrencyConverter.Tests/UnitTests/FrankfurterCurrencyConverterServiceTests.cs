using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.AppServices.Configuration.Dtos;
using CurrencyConverter.Services.AppServices.CustomizedHybridCache;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using Microsoft.Extensions.Caching.Hybrid;
using Moq;

namespace CurrencyConverter.Tests.UnitTests
{
    public class FrankfurterCurrencyConverterServiceTests
    {

        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly Mock<ICustomizedHybridCacheService> _cacheServiceMock;
        private readonly Mock<IConfigurationService> _configurationServiceMock;
        private readonly FrankfurterCurrencyConverterService _service;
        public FrankfurterCurrencyConverterServiceTests()
        {
            #region Mocking HttpClient
            // Initialize the HttpClient mock
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.frankfurter.app")
            };
            #endregion

            #region Mocking IConfigurationService

            // Initialize the IConfigurationService mock
            _configurationServiceMock = new Mock<IConfigurationService>();
            // Define a mock configuration object
            CurrencyConverterConfigurationDto mockConfig = new CurrencyConverterConfigurationDto
            {
                CurrencyConverterThirdPartyApi = new CurrencyConverterThirdPartyApiConfigurationDto()
                {
                    BaseUrl = "https://dummy.api",
                    AllowedCurrencyCodes = [
                                                        "AUD",
                                                        "BGN",
                                                        "BRL",
                                                        "CAD",
                                                        "CHF",
                                                        "CNY",
                                                        "CZK",
                                                        "DKK",
                                                        "EUR",
                                                        "GBP",
                                                        "HKD",
                                                        "HUF",
                                                        "IDR",
                                                        "ILS",
                                                        "INR",
                                                        "ISK",
                                                        "JPY",
                                                        "KRW",
                                                        "MYR",
                                                        "NOK",
                                                        "NZD",
                                                        "PHP",
                                                        "RON",
                                                        "SEK",
                                                        "SGD",
                                                        "USD",
                                                        "ZAR"
                                                      ],
                    HistoricalRatesCacheLifeTime = 60,
                    LatestRatesCacheLifeTime = 10
                }
            };
            // Set up the IConfigurationService mock to return the mock configuration
            _configurationServiceMock.Setup(c => c.Config)
                .Returns(mockConfig);

            #endregion

            #region Mocking ICustomizedHybridCacheService

            // Initialize the HybridCache mock
            _cacheServiceMock = new Mock<ICustomizedHybridCacheService>();

            #endregion
            // Initialize the service with the mocks
            _service = new FrankfurterCurrencyConverterService(
                httpClientFactory: Mock.Of<IHttpClientFactory>(f => f.CreateClient(It.IsAny<string>()) == httpClient),
                cache: _cacheServiceMock.Object,
                configurationService: _configurationServiceMock.Object
            );
        }

        #region GetLatestRatesAsync

        [Fact]
        public async Task GetLatestRatesAsync_ReturnsLatestRates_UsingCachedRates()
        {
            // Arrange
            var baseCurrency = "USD";

            // Set up the HybridCache mock
            CurrencyConverterLatestRatesServiceResponseDto cacheResult = new CurrencyConverterLatestRatesServiceResponseDto
            {
                Base = baseCurrency,
                Date = DateTime.UtcNow,
                Rates = new Dictionary<string, decimal> { { "EUR", 0.95438m }, { "AUD", 1.5761m } }
            };

            _cacheServiceMock.Setup(c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<CurrencyConverterLatestRatesServiceResponseDto>>>(),
                It.IsAny<HybridCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cacheResult);

            // Act
            CurrencyConverterLatestRatesServiceResponseDto result = await _service.GetLatestRatesAsync(baseCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(baseCurrency, result.Base);
            Assert.Equal(cacheResult.Rates, result.Rates);

        }

        [Fact]
        public async Task GetLatestRatesAsync_ThrowsException_WhenCurrencyIsBanned()
        {
            // Arrange
            var baseCurrency = "AED"; // Assume AED is banned

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => _service.GetLatestRatesAsync(baseCurrency));
        }

        #endregion

        #region ConvertCurrencyAsync

        [Fact]
        public async Task ConvertCurrencyAsync_ReturnsConvertedAmount_UsingCachedRates()
        {
            // Arrange
            var baseCurrency = "USD";
            var targetCurrency = "EUR";
            var amount = 100m;
            var expectedRate = 0.85m;

            var cachedRates = new CurrencyConverterLatestRatesServiceResponseDto
            {
                Base = baseCurrency,
                Date = DateTime.UtcNow,
                Rates = new Dictionary<string, decimal> { { targetCurrency, expectedRate } }
            };

            // Mock the cache to return cached rates
            _cacheServiceMock
                .Setup(c => c.GetOrCreateAsync(
                    It.Is<string>(key => key == string.Format("LatestRates_{0}", baseCurrency)),
                    It.IsAny<Func<CancellationToken, ValueTask<CurrencyConverterLatestRatesServiceResponseDto>>>(),
                    It.IsAny<HybridCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(cachedRates);

            // Act
            var result = await _service.ConvertCurrencyAsync(baseCurrency, amount, targetCurrency);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(amount * expectedRate, result.TargetAmount);
            Assert.Equal(baseCurrency, result.BaseCurrency);
            Assert.Equal(targetCurrency, result.TargetCurrency);
        }

        [Fact]
        public async Task ConvertCurrencyAsync_ThrowsException_WhenTargetCurrencyIsBanned()
        {
            // Arrange
            var baseCurrency = "USD";
            var targetCurrency = "AED"; // Assume AED is banned
            var amount = 100m;

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => _service.ConvertCurrencyAsync(baseCurrency, amount, targetCurrency));
        }

        [Fact]
        public async Task ConvertCurrencyAsync_ThrowsException_WhenBaseCurrencyIsBanned()
        {
            // Arrange
            var baseCurrency = "AED"; // Assume AED is banned
            var targetCurrency = "USD";
            var amount = 100m;

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => _service.ConvertCurrencyAsync(baseCurrency, amount, targetCurrency));
        }

        #endregion

        #region GetHistoricalRatesAsync

        [Fact]
        public async Task GetHistoricalRatesAsync_ReturnsHistoricalRates_UsingCachedData()
        {
            // Arrange
            var baseCurrency = "USD";
            var targetCurrency = "EUR";
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 31);
            var pageSize = 10;
            var pageNumber = 1;

            var cachedRates = new CurrencyConverterHistoricalRatesServiceResponseDto
            {
                Base = baseCurrency,
                Rates = new Dictionary<DateTime, Dictionary<string, decimal>>
                {
                    { startDate, new Dictionary<string, decimal> { { targetCurrency, 0.85m } } },
                    { endDate, new Dictionary<string, decimal> { { targetCurrency, 0.86m } } }
                }
            };

            // Mock the cache to return cached rates
            string startDateStr = startDate.ToString("yyyy-MM-dd");
            string endDateStr = endDate.ToString("yyyy-MM-dd");
            string cacheKey = string.Format("HistoricalRates_{0}_{1}_{2}_{3}", baseCurrency, targetCurrency, startDateStr, endDateStr);
            _cacheServiceMock
                .Setup(c => c.GetOrCreateAsync(
                    It.Is<string>(key => key == cacheKey),
                    It.IsAny<Func<CancellationToken, ValueTask<CurrencyConverterHistoricalRatesServiceResponseDto>>>(),
                    It.IsAny<HybridCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()
                ))
                .ReturnsAsync(cachedRates);

            // Act
            var result = await _service.GetHistoricalRatesAsync(baseCurrency, targetCurrency, startDate, endDate, pageSize, pageNumber);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(baseCurrency, result.Base);
            Assert.Equal(targetCurrency, result.Target);
            Assert.Equal(pageNumber, result.PageNumber);
            Assert.Equal(pageSize, result.PageSize);
            Assert.Equal(cachedRates.Rates.Count, result.TotalItems);
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ThrowsException_WhenBaseCurrencyIsBanned()
        {
            // Arrange
            var baseCurrency = "AED"; // Assume AED is banned
            var targetCurrency = "EUR";
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 31);
            var pageSize = 10;
            var pageNumber = 1;

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => _service.GetHistoricalRatesAsync(baseCurrency, targetCurrency, startDate, endDate, pageSize, pageNumber));
        }

        [Fact]
        public async Task GetHistoricalRatesAsync_ThrowsException_WhenTargetCurrencyIsBanned()
        {
            // Arrange
            var baseCurrency = "EUR"; 
            var targetCurrency = "AED"; // Assume AED is banned
            var startDate = new DateTime(2023, 1, 1);
            var endDate = new DateTime(2023, 1, 31);
            var pageSize = 10;
            var pageNumber = 1;

            // Act & Assert
            await Assert.ThrowsAsync<AppException>(() => _service.GetHistoricalRatesAsync(baseCurrency, targetCurrency, startDate, endDate, pageSize, pageNumber));
        }

        #endregion

    }
}
