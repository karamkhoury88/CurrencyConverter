using CurrencyConverter.Api.Dtos.Bases.Responses;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.CurrencyConverter.Responses
{
    /// <summary>
    /// Represents the data transfer object for a response containing the latest exchange rates.
    /// </summary>
    public record GetLatestRatesResponseAppDto : BaseResponseAppDTO
    {
        /// <summary>
        /// The base currency code for which the latest rates are fetched (e.g., "EUR").
        /// </summary>
        [JsonPropertyName("base")]
        [Required]
        public required string Base { get; init; }

        /// <summary>
        /// The date on which the exchange rates were recorded.
        /// </summary>
        [JsonPropertyName("date")]
        [Required]
        public required DateTime Date { get; init; }

        /// <summary>
        /// A dictionary containing the latest exchange rates, where the key is the currency code and the value is the exchange rate.
        /// </summary>
        [JsonPropertyName("rates")]
        [Required]
        public required Dictionary<string, decimal> Rates { get; init; } = [];

        /// <summary>
        /// Creates a <see cref="GetLatestRatesResponseAppDto"/> from a service-level DTO.
        /// </summary>
        /// <param name="svcDto">The service-level DTO containing the latest rates data.</param>
        /// <returns>A new instance of <see cref="GetLatestRatesResponseAppDto"/>.</returns>
        public static GetLatestRatesResponseAppDto FromSvcDto(CurrencyConverterLatestRatesServiceResponseDto svcDto)
        {
            return new()
            {
                Base = svcDto.Base,
                Date = svcDto.Date,
                Rates = svcDto.Rates,
            };
        }
    }
}