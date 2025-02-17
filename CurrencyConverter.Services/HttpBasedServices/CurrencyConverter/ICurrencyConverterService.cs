using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter
{
    /// <summary>
    /// Interface for the Currency Converter service.
    /// Provides methods for fetching latest rates, converting currencies, and retrieving historical rates.
    /// </summary>
    public interface ICurrencyConverterService
    {
        /// <summary>
        /// Fetches the latest working day's rates, updated daily around 16:00 CET.
        /// </summary>
        /// <param name="baseCurrency">The base currency code.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the latest rates.</returns>
        Task<CurrencyConverterLatestRatesServiceResponseDto> GetLatestRatesAsync(string baseCurrency);

        /// <summary>
        /// Performs currency conversion between base and target currencies.
        /// </summary>
        /// <param name="baseCurrency">The base currency code.</param>
        /// <param name="amount">The amount to convert.</param>
        /// <param name="targetCurrency">The target currency code.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the conversion result.</returns>
        Task<CurrencyConverterConvertingServiceResponseDto> ConvertCurrencyAsync(string baseCurrency, decimal amount, string targetCurrency);

        /// <summary>
        /// Retrieves paginated rates for base currency and target currency over a period.
        /// </summary>
        /// <param name="baseCurrency">The base currency code.</param>
        /// <param name="targetCurrency">The target currency code.</param>
        /// <param name="startDate">The start date of the period.</param>
        /// <param name="endDate">The end date of the period.</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <param name="pageNumber">The number of the page to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the paginated historical rates.</returns>
        Task<CurrencyConverterHistoricalRatesPagedServiceResponseDto> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, DateTime startDate, DateTime endDate, int pageSize, int pageNumber);
    }
}
