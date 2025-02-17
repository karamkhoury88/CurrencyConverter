using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.ServiceDefaults.Helpers;
using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.AppServices.CustomizedHybridCache;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos.Frankfurter;
using Microsoft.Extensions.Caching.Hybrid;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter
{
    /// <summary>
    /// Service for converting currencies using the Frankfurter API.
    /// Implements the <see cref="ICurrencyConverterService"/> interface.
    /// </summary>
    public class FrankfurterCurrencyConverterService : ICurrencyConverterService
    {
        private readonly HttpClient _httpClient; // HTTP client for making API requests
        private readonly ICustomizedHybridCacheService _cache; // Cache service for storing rates
        private readonly IConfigurationService _configuration; // Configuration service for accessing settings

        // Cache key templates
        private const string _latestRatesCacheKey = "LatestRates_{0}";
        private const string _latestRatesForSymbolCacheKey = "LatestRatesForSymbol_{0}_{1}";
        private const string _historicalRates_CacheKey = "HistoricalRates_{0}_{1}_{2}_{3}";

        /// <summary>
        /// Initializes a new instance of the <see cref="FrankfurterCurrencyConverterService"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
        /// <param name="cache">Cache service for storing and retrieving cached data.</param>
        /// <param name="configurationService">Configuration service for accessing application settings.</param>
        public FrankfurterCurrencyConverterService(IHttpClientFactory httpClientFactory, ICustomizedHybridCacheService cache, IConfigurationService configurationService)
        {
            _cache = cache;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configurationService;

            // Set the base address for the HTTP client from configuration
            _httpClient.BaseAddress = new Uri(_configuration.Config.CurrencyConverterThirdPartyApi.BaseUrl);
        }

        /// <summary>
        /// Retrieves the latest currency exchange rates for a specified base currency.
        /// </summary>
        /// <param name="baseCurrency">The base currency code (e.g., "USD").</param>
        /// <returns>A <see cref="CurrencyConverterLatestRatesServiceResponseDto"/> containing the latest rates.</returns>
        /// <exception cref="AppException">Thrown if the base currency is banned or the API call fails.</exception>
        public async Task<CurrencyConverterLatestRatesServiceResponseDto> GetLatestRatesAsync(string baseCurrency)
        {
            if (IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                    nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                    technicalMessage: $"{baseCurrency} currency is not allowed");
            }

            // Generate cache key for the latest rates
            string cacheKey = string.Format(_latestRatesCacheKey, baseCurrency);

            // Retrieve or create cached rates
            return await _cache.GetOrCreateAsync<CurrencyConverterLatestRatesServiceResponseDto>(
                key: cacheKey,
                factory: GetLatestRatesFactory,
                options: new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(_configuration.Config.CurrencyConverterThirdPartyApi.LatestRatesCacheLifeTime)
                },
                cancellationToken: default);

            // Factory method for fetching and processing the latest rates
            async ValueTask<CurrencyConverterLatestRatesServiceResponseDto> GetLatestRatesFactory(CancellationToken token)
            {
                // Fetch latest rates from the API
                FrankfurterGetLatestRatesServiceResponseDto frankfurterResponse = await EnsureSuccessStatusCodeAsync<FrankfurterGetLatestRatesServiceResponseDto>(
                    await _httpClient.GetAsync($"latest?base={baseCurrency}", token),
                    token);

                // Filter out banned currencies from the response
                var bannedKeys = frankfurterResponse.Rates.Keys
                    .Where(x => !_configuration.Config.CurrencyConverterThirdPartyApi.AllowedCurrencyCodes.Contains(x));

                foreach (var bannedKey in bannedKeys)
                {
                    frankfurterResponse.Rates.Remove(bannedKey);
                }

                // Map the response to the service DTO
                return new CurrencyConverterLatestRatesServiceResponseDto()
                {
                    Base = frankfurterResponse.Base,
                    Date = frankfurterResponse.Date,
                    Rates = frankfurterResponse.Rates
                };
            }
        }

        /// <summary>
        /// Converts an amount from one currency to another using the latest exchange rates.
        /// </summary>
        /// <param name="baseCurrency">The source currency code.</param>
        /// <param name="amount">The amount to convert.</param>
        /// <param name="targetCurrency">The target currency code.</param>
        /// <returns>A <see cref="CurrencyConverterConvertingServiceResponseDto"/> containing the conversion result.</returns>
        /// <exception cref="AppException">Thrown if either currency is banned or the API call fails.</exception>
        public async Task<CurrencyConverterConvertingServiceResponseDto> ConvertCurrencyAsync(string baseCurrency, decimal amount, string targetCurrency)
        {
            if (IsCurrencyBanned(baseCurrency) || IsCurrencyBanned(targetCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION, $"Conversion involving {baseCurrency}, {targetCurrency} is not allowed.", technicalMessage: "");
            }

            // Initialize the response DTO
            CurrencyConverterConvertingServiceResponseDto svcResponse = new()
            {
                BaseAmount = amount,
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
            };

            // If base and target currencies are the same, no conversion is needed
            if (baseCurrency == targetCurrency)
            {
                svcResponse.TargetAmount = amount;
                return svcResponse;
            }

            // Generate cache key for the latest rates of the base currency
            string latestRatesCacheKey = string.Format(_latestRatesCacheKey, baseCurrency);

            // Attempt to retrieve cached rates for the base currency
            CurrencyConverterLatestRatesServiceResponseDto? baseCurrencyLatestRate = await _cache.GetOrCreateAsync<CurrencyConverterLatestRatesServiceResponseDto?>(latestRatesCacheKey,
            factory: cancel =>
            {
                return default; // Return a default (do NOT cache this value)
            },
            cancellationToken: CancellationToken.None, // Optional,
            options: new HybridCacheEntryOptions() { Flags = HybridCacheEntryFlags.DisableUnderlyingData });

            // If cached rates are available and contain the target currency, perform the conversion
            if (baseCurrencyLatestRate != null && baseCurrencyLatestRate.Rates.TryGetValue(targetCurrency, out decimal targetCurrencyRate))
            {
                svcResponse.TargetAmount = amount * targetCurrencyRate;
                return svcResponse;
            }
            else
            {
                // Generate cache key for the latest rates of the base currency and target currency
                var latestRatesForSymbolCacheKey = string.Format(_latestRatesForSymbolCacheKey, baseCurrency, targetCurrency);

                // Fetch and cache the latest rates for the base and target currencies
                CurrencyConverterLatestRatesServiceResponseDto baseCurrencyLatestRateForSymbol = await _cache.GetOrCreateAsync<CurrencyConverterLatestRatesServiceResponseDto>(latestRatesForSymbolCacheKey,
                async token =>
                {
                    FrankfurterGetLatestRatesServiceResponseDto frankfurterResponse = await EnsureSuccessStatusCodeAsync<FrankfurterGetLatestRatesServiceResponseDto>(
                        await _httpClient.GetAsync($"latest?base={baseCurrency}&symbols={targetCurrency}", token),
                        token);

                    return new CurrencyConverterLatestRatesServiceResponseDto()
                    {
                        Base = frankfurterResponse.Base,
                        Date = frankfurterResponse.Date,
                        Rates = frankfurterResponse.Rates,
                    };
                },
                options: new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(_configuration.Config.CurrencyConverterThirdPartyApi.LatestRatesCacheLifeTime)
                });

                // Perform the conversion using the fetched rates
                if (baseCurrencyLatestRateForSymbol.Rates.TryGetValue(targetCurrency, out targetCurrencyRate))
                {
                    svcResponse.TargetAmount = amount * targetCurrencyRate;
                    return svcResponse;
                }
                else
                {
                    throw new AppException(AppErrorCode.CURRENCY_CONVERTER_NOT_SUPPORTED_CURRENCY, "We are facing some troubles, please try again.", technicalMessage: $"Target or based currencies, {targetCurrency}, {baseCurrency} are not supported");
                }
            }
        }

        /// <summary>
        /// Retrieves historical exchange rates for a specified date range and currencies.
        /// </summary>
        /// <param name="baseCurrency">The base currency code.</param>
        /// <param name="targetCurrency">The target currency code.</param>
        /// <param name="startDate">The start date of the historical range.</param>
        /// <param name="endDate">The end date of the historical range.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <param name="pageNumber">The page number to retrieve.</param>
        /// <returns>A <see cref="CurrencyConverterHistoricalRatesPagedServiceResponseDto"/> containing the paginated historical rates.</returns>
        /// <exception cref="AppException">Thrown if either currency is banned or the API call fails.</exception>
        public async Task<CurrencyConverterHistoricalRatesPagedServiceResponseDto> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, DateTime startDate, DateTime endDate, int pageSize, int pageNumber)
        {
            if (IsCurrencyBanned(baseCurrency) || IsCurrencyBanned(targetCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION, $"Rates involving {baseCurrency}, {targetCurrency} is not allowed.", technicalMessage: "");
            }

            // Format dates for the API request
            string startDateStr = $"{startDate:yyyy-MM-dd}";
            string endDateStr = $"{endDate:yyyy-MM-dd}";

            string dateRangeStr = $"{startDateStr}..{endDateStr}";

            // Generate cache key for historical rates
            string cacheKey = string.Format(_historicalRates_CacheKey, baseCurrency, targetCurrency, startDateStr, endDateStr);

            // Retrieve or create cached historical rates
            CurrencyConverterHistoricalRatesServiceResponseDto totalItems = await _cache.GetOrCreateAsync<CurrencyConverterHistoricalRatesServiceResponseDto>(cacheKey,
               async token =>
               {
                   FrankfurterHistoricalRatesServiceResponseDto frankfurterResponse = await EnsureSuccessStatusCodeAsync<FrankfurterHistoricalRatesServiceResponseDto>(
                        await _httpClient.GetAsync($"{dateRangeStr}?base={baseCurrency}&symbols={targetCurrency}", token),
                        token);

                   return new CurrencyConverterHistoricalRatesServiceResponseDto
                   {
                       Base = frankfurterResponse.Base,
                       Rates = frankfurterResponse.Rates,
                   };
               },
           options: new HybridCacheEntryOptions
           {
               Expiration = TimeSpan.FromMinutes(_configuration.Config.CurrencyConverterThirdPartyApi.HistoricalRatesCacheLifeTime)
           });

            // Paginate the historical rates
            var paginatedRates = PaginationHelper.PaginateDictionary(totalItems.Rates, pageNumber, pageSize);

            // Prepare the response DTO
            CurrencyConverterHistoricalRatesPagedServiceResponseDto response = new()
            {
                Base = baseCurrency,
                Target = targetCurrency,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems.Rates.Count,
                TotalPages = (int)Math.Ceiling(totalItems.Rates.Count / (double)pageSize),
                Rates = paginatedRates
            };

            return response;
        }

        #region Privates

        /// <summary>
        /// Checks if a currency is banned based on configuration.
        /// </summary>
        /// <param name="currency">The currency code to check.</param>
        /// <returns>True if the currency is banned; otherwise, false.</returns>
        private bool IsCurrencyBanned(string currency) => _configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(currency);

        /// <summary>
        /// Ensures that the HTTP response is successful and deserializes the response content.
        /// </summary>
        /// <typeparam name="T">The type to deserialize the response content into.</typeparam>
        /// <param name="response">The HTTP response message.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The deserialized response content.</returns>
        /// <exception cref="AppException">Thrown if the response is not successful or deserialization fails.</exception>
        private static async Task<T> EnsureSuccessStatusCodeAsync<T>(HttpResponseMessage? response, CancellationToken cancellationToken)
        {
            if (response is null || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new AppException(AppErrorCode.CURRENCY_CONVERTER_THIRD_PARTY_SYSTEM_FAILURE,
                    nonTechnicalMessage: "We are facing some troubles, please try again.",
                    technicalMessage: "The status code from Frankfurter api is not HttpStatusCode.OK");
            }

            T? svcResponse = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            return svcResponse is null
                ? throw new AppException(AppErrorCode.CURRENCY_CONVERTER_THIRD_PARTY_SYSTEM_FAILURE,
                nonTechnicalMessage: "We are facing some troubles, please try again.",
                technicalMessage: "The response content from Frankfurter api is not compatible")
                : svcResponse;
        }

        #endregion
    }
}