
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos.Frankfurter;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter
{
    public interface ICurrencyConverterService
    {
        /// <summary>
        /// Fetch the latest working day's rates, updated daily around 16:00 CET.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <returns></returns>
        Task<CurrencyConverterLatestRatesServiceResponseDto> GetLatestRatesAsync(string baseCurrency);

        /// <summary>
        /// Perform currency conversion between base and target currencies.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="amount"></param>
        /// <param name="targetCurrency"></param>
        /// <returns></returns>
        Task<CurrencyConverterConvertingServiceResponseDto> ConvertCurrencyAsync(string baseCurrency, decimal amount, string targetCurrency);

        /// <summary>
        /// Retrieve paginated rates for base currency and target currency over a period.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="targetCurrency"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        Task<CurrencyConverterHistoricalRatesPagedServiceResponseDto> GetHistoricalRatesAsync(string baseCurrency, string targetCurrency, DateTime startDate, DateTime endDate, int pageSize, int pageNumber);       
    }
}
