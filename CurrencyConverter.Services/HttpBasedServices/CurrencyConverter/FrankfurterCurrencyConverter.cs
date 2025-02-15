using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.ServiceDefaults.Helpers;
using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos.Frankfurter;
using Microsoft.Extensions.Caching.Hybrid;
using System.Collections.Immutable;
using System.Net.Http.Json;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter
{
    internal class FrankfurterCurrencyConverter : ICurrencyConverterService
    {
        private readonly HttpClient _httpClient;
        private readonly HybridCache _cache;
        private readonly IConfigurationService _configuration;
        private const string _latestRatesCacheKey = "LatestRates_{0}";
        private const string _latestRatesForSymbolCacheKey = "LatestRatesForSymbol_{0}_{1}";
        private const string _historicalRates_CacheKey = "HistoricalRates_{0}_{1}_{2}_{3}";

        public FrankfurterCurrencyConverter(IHttpClientFactory httpClientFactory, HybridCache cache, IConfigurationService configurationService)
        {
            _cache = cache;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configurationService;

            _httpClient.BaseAddress = new Uri(_configuration.Config.CurrencyConverterThirdPartyApi.BaseUrl);
        }

        public async Task<CurrencyConverterLatestRatesServiceResponseDto> GetLatestRatesAsync(string baseCurrency)
        {
            if (IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                    nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                    technicalMessage: $"{baseCurrency} currency is not allowed");
            }

            string cacheKey = string.Format(_latestRatesCacheKey, baseCurrency);


            return await _cache.GetOrCreateAsync<CurrencyConverterLatestRatesServiceResponseDto>(cacheKey,
                async token =>
                {
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


                    return new CurrencyConverterLatestRatesServiceResponseDto()
                    {
                        Base = frankfurterResponse.Base,
                        Date = frankfurterResponse.Date,
                        Rates = frankfurterResponse.Rates
                    };
                },
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(_configuration.Config.CurrencyConverterThirdPartyApi.LatestRatesCacheLifeTime)
            });
        }

        public async Task<CurrencyConverterConvertingServiceResponseDto> ConvertCurrencyAsync(string baseCurrency, decimal amount, string targetCurrency)
        {
            if (IsCurrencyBanned(baseCurrency) || IsCurrencyBanned(targetCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION, $"Conversion involving {baseCurrency}, {targetCurrency} is not allowed.", technicalMessage: "");
            }

            CurrencyConverterConvertingServiceResponseDto svcResponse = new()
            {
                BaseAmount = amount,
                BaseCurrency = baseCurrency,
                TargetCurrency = targetCurrency,
            };

            string latestRatesCacheKey = string.Format(_latestRatesCacheKey, baseCurrency);

            /*
            1 - check if Latest rate for base currency to all currencies (including the target) is in the cache, so we convert based on it
            2-  else, fetch the latest rate for base currency and filter to only one symbol (the target), cache it and convert based on it
                  . Note: we never cache the conversion result, instead we cache the latest rate ob base currency and target currency
            */

            //Only fetches the value from cache, without access the underlying data store.
            CurrencyConverterLatestRatesServiceResponseDto? baseCurrencyLatestRate = await _cache.GetOrCreateAsync<CurrencyConverterLatestRatesServiceResponseDto?>(latestRatesCacheKey,
            factory: cancel =>
            {
                return default; // Return a default (do NOT cache this value)
            },
            cancellationToken: CancellationToken.None, // Optional,
            options: new HybridCacheEntryOptions() { Flags = HybridCacheEntryFlags.DisableUnderlyingData });

            if (baseCurrencyLatestRate != null && baseCurrencyLatestRate.Rates.TryGetValue(targetCurrency, out decimal targetCurrencyRate))
            {
                svcResponse.TargetAmount = amount * targetCurrencyRate;
                return svcResponse;
            }
            else
            {
                var latestRatesForSymbolCacheKey = string.Format(_latestRatesForSymbolCacheKey, baseCurrency, targetCurrency);

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

        public async Task<CurrencyConverterHistoricalRatesPagedServiceResponseDto> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, DateTime startDate, DateTime endDate, int pageSize, int pageNumber)
        {
            if (IsCurrencyBanned(baseCurrency) || IsCurrencyBanned(targetCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION, "Rates involving TRY, PLN, THB, or MXN are not allowed.", technicalMessage: "");
            }

            string startDateStr = $"{startDate:yyyy-MM-dd}";
            string endDateStr = $"{endDate:yyyy-MM-dd}";

            string dateRangeStr = $"{startDateStr}..{endDateStr}";

            string cacheKey = string.Format(_historicalRates_CacheKey, baseCurrency, targetCurrency, startDateStr, endDateStr);

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

            var paginatedRates = PaginationHelper.PaginateDictionary(totalItems.Rates, pageNumber, pageSize);

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

        private bool IsCurrencyBanned(string currency) => _configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(currency);

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
