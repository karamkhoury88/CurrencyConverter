using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
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

        // TODO: Move banned currencies to the configurations.
        private readonly ImmutableHashSet<string> _bannedCurrencies = ["TRY", "PLN", "THB", "MXN"];

        public FrankfurterCurrencyConverter(IHttpClientFactory httpClientFactory, HybridCache cache, IConfigurationService configurationService)
        {
            _cache = cache;
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configurationService;

            _httpClient.BaseAddress = new Uri(_configuration.Config.CurrencyConverterThirdPartyApi.BaseUrl);
        }

        public async Task<ConvertCurrencyServiceResponseDto> ConvertCurrencyAsync(string baseCurrency, decimal amount, string targetCurrency)
        {
            if (IsCurrencyBanned(baseCurrency) || IsCurrencyBanned(targetCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION, "Conversion involving TRY, PLN, THB, or MXN is not allowed.");
            }

            ConvertCurrencyServiceResponseDto svcResponse = new()
            {
                FromAmount = amount,
                FromCurrency = baseCurrency,
                ToCurrency = targetCurrency,
            };
            decimal targetCurrencyRate;


            string latestRatesCacheKey = string.Format(_latestRatesCacheKey, baseCurrency);

            //First, check if Latest rate for base currency to all currencies (including the target) is in the cache, so we convert based on it
            //Else, Fetch the latest rate for base currency and filter to only one symbol (the target), cache it and convert based on it

            //Only fetches the value from cache; does not attempt to access the underlying data store.


            GetLatestRatesServiceResponseDto? baseCurrencyLatestRate = await _cache.GetOrCreateAsync<GetLatestRatesServiceResponseDto?>(latestRatesCacheKey,
            factory: async cancel =>
            {
                // This factory runs ONLY if the key is NOT in the cache.
                return default; // Return a default (do NOT cache this value)
            },
            cancellationToken: CancellationToken.None, // Optional,
            options: new HybridCacheEntryOptions() { Flags = HybridCacheEntryFlags.DisableUnderlyingData });

            if (baseCurrencyLatestRate != null && baseCurrencyLatestRate.Rates.TryGetValue(targetCurrency, out targetCurrencyRate))
            {
                svcResponse.ToAmount = amount * targetCurrencyRate;
                return svcResponse;
            }
            else
            {
                var latestRatesForSymbolCacheKey = string.Format(_latestRatesForSymbolCacheKey, baseCurrency, targetCurrency);

                GetLatestRatesServiceResponseDto baseCurrencyLatestRateForSymbol = await _cache.GetOrCreateAsync(latestRatesForSymbolCacheKey,
                async token =>
                {
                    return await EnsureSuccessStatusCodeAsync<GetLatestRatesServiceResponseDto>(
                        await _httpClient.GetAsync($"latest?base={baseCurrency}&symbols={targetCurrency}", token),
                        token);
                },
                options: new HybridCacheEntryOptions
                {
                    Expiration = TimeSpan.FromMinutes(_configuration.Config.CurrencyConverterThirdPartyApi.LatestRatesCacheLifeTime)
                });

                if (baseCurrencyLatestRateForSymbol.Rates.TryGetValue(targetCurrency, out targetCurrencyRate))
                {
                    svcResponse.ToAmount = amount * targetCurrencyRate;
                    return svcResponse;
                }
                else
                {
                    throw new AppException(AppErrorCode.CURRENCY_CONVERTER_NOT_SUPPORTED_CURRENCY, "We are facing some troubles, please try again.");
                }
            }
        }

        public async Task<GetLatestRatesServiceResponseDto> GetLatestRatesAsync(string baseCurrency)
        {
            if (IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION, "Rates involving TRY, PLN, THB, or MXN are not allowed.");
            }

            string cacheKey = string.Format(_latestRatesCacheKey, baseCurrency);


            return await _cache.GetOrCreateAsync(cacheKey,
                async token =>
            {
                GetLatestRatesServiceResponseDto svcResponse = await EnsureSuccessStatusCodeAsync<GetLatestRatesServiceResponseDto>(
                    await _httpClient.GetAsync($"latest?base={baseCurrency}", token),
                    token);

                // Filter out banned currencies from the response
                foreach (var bannedCurrency in _bannedCurrencies)
                {
                    svcResponse.Rates.Remove(bannedCurrency);
                }

                return svcResponse;
            },
            options: new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(_configuration.Config.CurrencyConverterThirdPartyApi.LatestRatesCacheLifeTime)
            });
        }

        public async Task<HistoricalRatesServiceResponseDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate)
        {
            throw new NotImplementedException();
        }

        #region Privates

        private bool IsCurrencyBanned(string currency) => _bannedCurrencies.Contains(currency, StringComparer.OrdinalIgnoreCase);

        private static async Task<T> EnsureSuccessStatusCodeAsync<T>(HttpResponseMessage? response, CancellationToken cancellationToken)
        {
            if (response is null || response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new AppException(AppErrorCode.CURRENCY_CONVERTER_THIRD_PARTY_SYSTEM_FAILURE, "We are facing some troubles, please try again.");
            }

            T? svcResponse = await response.Content.ReadFromJsonAsync<T>(cancellationToken);
            return svcResponse is null
                ? throw new AppException(AppErrorCode.CURRENCY_CONVERTER_THIRD_PARTY_SYSTEM_FAILURE, "We are facing some troubles, please try again.")
                : svcResponse;
        }
        #endregion
    }
}
