using Asp.Versioning;
using CurrencyConverter.Api.Dtos.CurrencyConverter.Responses;
using CurrencyConverter.ServiceDefaults.Constants;
using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Factories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Api.Controllers
{
    /// <summary>
    /// Controller for handling currency conversion and exchange rate-related operations.
    /// </summary>
    [ApiVersion("1.0")]
    public class CurrencyConverterController(ICurrencyConverterFactory currencyConverterFactory,
        IConfigurationService configuration,
        ILogger<AuthController> logger) : AppBaseController
    {
        /// <summary>
        /// Retrieves the latest exchange rates for the specified base currency against all other currencies.
        /// </summary>
        /// <param name="baseCurrency">The currency code for which to fetch exchange rates.</param>
        /// <param name="provider">The provider to use for fetching rates (default is Frankfurter).</param>
        /// <returns>The latest exchange rates for the base currency.</returns>
        /// <exception cref="AppException">Thrown if the base currency is not allowed.</exception>
        [HttpGet("rates/latest")]
        [Authorize]
        [ProducesResponseType(typeof(GetLatestRatesResponseAppDto), 200)]
        public async Task<IActionResult> GetLatestRatesAsync([FromQuery, Required] string baseCurrency,
            [FromQuery] string provider = CurrencyConverterProviders.FRANKFURTER)
        {
            // Check if the base currency is banned.
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                   technicalMessage: $"{baseCurrency} currency is not allowed");
            }

            // Get the appropriate currency converter and fetch the latest rates.
            var converter = currencyConverterFactory.GetConverter(provider);
            return Ok(GetLatestRatesResponseAppDto.FromSvcDto(await converter.GetLatestRatesAsync(baseCurrency)));
        }

        /// <summary>
        /// Converts a specified amount from the base currency to the target currency.
        /// </summary>
        /// <param name="baseCurrency">The currency code to convert from.</param>
        /// <param name="targetCurrency">The currency code to convert to.</param>
        /// <param name="amount">The amount to convert.</param>
        /// <param name="provider">The provider to use for conversion (default is Frankfurter).</param>
        /// <returns>The converted amount in the target currency.</returns>
        /// <exception cref="AppException">Thrown if the base or target currency is not allowed.</exception>
        [HttpGet("convert")]
        [Authorize]
        [ProducesResponseType(typeof(ConvertResponseAppDto), 200)]
        public async Task<IActionResult> ConvertAsync([FromQuery, Required] string baseCurrency,
            [FromQuery, Required] string targetCurrency,
            [FromQuery, Required, Range(0.1, double.MaxValue)] decimal amount,
            [FromQuery] string provider = CurrencyConverterProviders.FRANKFURTER)
        {
            // Check if the base currency is banned.
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                   technicalMessage: $"{baseCurrency} currency is not allowed");
            }

            // Check if the target currency is banned.
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(targetCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{targetCurrency} currency is not allowed",
                   technicalMessage: $"{targetCurrency} currency is not allowed");
            }

            // Get the appropriate currency converter and perform the conversion.
            var converter = currencyConverterFactory.GetConverter(provider);
            return Ok(ConvertResponseAppDto.FromSvcDto(await converter.ConvertCurrencyAsync(baseCurrency, amount, targetCurrency)));
        }

        /// <summary>
        /// Retrieves historical exchange rates for the specified base and target currencies within a date range.
        /// </summary>
        /// <param name="baseCurrency">The base currency code.</param>
        /// <param name="targetCurrency">The target currency code.</param>
        /// <param name="startDate">The start date of the historical data range.</param>
        /// <param name="endDate">The end date of the historical data range.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="pageNumber">The page number to retrieve (starting from 1).</param>
        /// <param name="provider">The provider to use for fetching historical rates (default is Frankfurter).</param>
        /// <returns>The historical exchange rates for the specified currencies and date range.</returns>
        /// <exception cref="AppException">Thrown if the base currency is not allowed or if the date range is invalid.</exception>
        [HttpGet("rates/paged")]
        [Authorize]
        [ProducesResponseType(typeof(GetHistoricalRatesResponseAppDto), 200)]
        public async Task<IActionResult> GetHistoricalRatesAsync([FromQuery, Required] string baseCurrency,
           [FromQuery, Required] string targetCurrency,
           [FromQuery, Required] DateTime startDate,
           [FromQuery, Required] DateTime endDate,
           [FromQuery, Required, Range(1, 50)] int pageSize,
           [FromQuery, Required, Range(1, int.MaxValue)] int pageNumber,
           [FromQuery] string provider = CurrencyConverterProviders.FRANKFURTER)
        {
            // Check if the base currency is banned.
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                   technicalMessage: $"{baseCurrency} currency is not allowed");
            }

            // Validate the date range.
            if (startDate >= endDate)
            {
                throw new AppException(AppErrorCode.INVALID_PARAMETER, "Start date must be before end date");
            }

            // Get the appropriate currency converter and fetch the historical rates.
            var converter = currencyConverterFactory.GetConverter(provider);
            return Ok(GetHistoricalRatesResponseAppDto.FromSvcDto(await converter.GetHistoricalRatesAsync(baseCurrency: baseCurrency,
                targetCurrency: targetCurrency,
                startDate: startDate,
                endDate: endDate,
                pageSize: pageSize,
                pageNumber: pageNumber)));
        }
    }
}