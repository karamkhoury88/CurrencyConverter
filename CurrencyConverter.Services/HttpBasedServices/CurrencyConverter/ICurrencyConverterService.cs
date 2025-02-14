
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter
{
    public interface ICurrencyConverterService
    {
        /// <summary>
        /// Fetch the latest working day's rates, updated daily around 16:00 CET.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <returns></returns>
        Task<GetLatestRatesServiceResponseDto> GetLatestRatesAsync(string baseCurrency);

        /// <summary>
        /// Retrieve rates over a period.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        Task<HistoricalRatesServiceResponseDto> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Perform currency conversion by fetching the exchange rate and calculating in our side.
        /// </summary>
        /// <param name="baseCurrency"></param>
        /// <param name="amount"></param>
        /// <param name="targetCurrency"></param>
        /// <returns></returns>
        Task<ConvertCurrencyServiceResponseDto> ConvertCurrencyAsync(string baseCurrency, decimal amount, string targetCurrency);

    }
}
