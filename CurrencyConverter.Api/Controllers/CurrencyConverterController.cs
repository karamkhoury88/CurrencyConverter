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
    [ApiVersion("1.0")]
    public class CurrencyConverterController(ICurrencyConverterFactory currencyConverterFactory,
        IConfigurationService configuration,
        ILogger<AuthController> logger) : AppBaseController
    {

        /// <summary>
        /// Get latest rates of the base currency against all other currencies.
        /// </summary>
        /// <param name="baseCurrency">The currency you want to fetch rates for.</param>
        /// <param name="provider">e.g. Frankfurter</param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpGet("rates/latest")]
        [Authorize]
        [ProducesResponseType(typeof(GetLatestRatesResponseAppDto), 200)]
        public async Task<IActionResult> GetLatestRatesAsync([FromQuery, Required] string baseCurrency,
            [FromQuery] string provider = CurrencyConverterProviders.FRANKFURTER)
        {
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                   technicalMessage: $"{baseCurrency} currency is not allowed");
            }

            var converter = currencyConverterFactory.GetConverter(provider);
            return Ok(GetLatestRatesResponseAppDto.FromSvcDto(await converter.GetLatestRatesAsync(baseCurrency)));
        }


        /// <summary>
        /// Convert any amount from the base currency to the target currency.
        /// </summary>
        /// <param name="baseCurrency">The currency you want to convert from</param>
        /// <param name="targetCurrency">The currency you want to convert to</param>
        /// <param name="amount">The amount of base currency</param>
        /// <param name="provider"> e.g. Frankfurter</param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
        [HttpGet("convert")]
        [Authorize]
        [ProducesResponseType(typeof(ConvertResponseAppDto), 200)]
        public async Task<IActionResult> ConvertAsync([FromQuery, Required] string baseCurrency,
            [FromQuery, Required] string targetCurrency,
            [FromQuery, Required, Range(0.1, double.MaxValue)] decimal amount,
            [FromQuery] string provider = CurrencyConverterProviders.FRANKFURTER)
        {
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                   technicalMessage: $"{baseCurrency} currency is not allowed");
            }
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(targetCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{targetCurrency} currency is not allowed",
                   technicalMessage: $"{targetCurrency} currency is not allowed");
            }

            var converter = currencyConverterFactory.GetConverter(provider);
            return Ok(ConvertResponseAppDto.FromSvcDto(await converter.ConvertCurrencyAsync(baseCurrency, amount, targetCurrency)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseCurrency">The currency you want to fetch rates for.</param>
        /// <param name="targetCurrency">The currency you want to fetch rates from.</param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="pageSize">Number of records required in the page</param>
        /// <param name="pageNumber">The page index (starting from 1)</param>
        /// <param name="provider"> e.g. Frankfurter</param>
        /// <returns></returns>
        /// <exception cref="AppException"></exception>
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
            if (configuration.Config.CurrencyConverterThirdPartyApi.IsCurrencyBanned(baseCurrency))
            {
                throw new AppException(AppErrorCode.NOT_ALLOWED_OPERATION,
                   nonTechnicalMessage: $"{baseCurrency} currency is not allowed",
                   technicalMessage: $"{baseCurrency} currency is not allowed");
            }


            if (startDate >= endDate)
            {
                throw new AppException(AppErrorCode.INVALID_PARAMETER, "Start date must be before end date");
            }

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
