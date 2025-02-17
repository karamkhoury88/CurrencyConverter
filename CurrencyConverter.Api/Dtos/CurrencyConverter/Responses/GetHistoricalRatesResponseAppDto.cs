using CurrencyConverter.Api.Dtos.Bases.Responses;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.CurrencyConverter.Responses
{
    /// <summary>
    /// Represents the data transfer object for a response containing historical exchange rates.
    /// </summary>
    public record GetHistoricalRatesResponseAppDto : BaseResponseAppDTO
    {
        /// <summary>
        /// The base currency code for which historical rates are fetched.
        /// </summary>
        [JsonPropertyName("base")]
        [Required]
        public required string Base { get; init; }

        /// <summary>
        /// The target currency code for which historical rates are fetched.
        /// </summary>
        [JsonPropertyName("target")]
        [Required]
        public required string Target { get; init; }

        /// <summary>
        /// The current page number in the paginated results.
        /// </summary>
        [JsonPropertyName("pageNumber")]
        [Required]
        public required int PageNumber { get; init; }

        /// <summary>
        /// The number of records per page in the paginated results.
        /// </summary>
        [JsonPropertyName("pageSize")]
        [Required]
        public required int PageSize { get; init; }

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        [JsonPropertyName("totalItems")]
        [Required]
        public required int TotalItems { get; init; }

        /// <summary>
        /// The total number of pages in the paginated results.
        /// </summary>
        [JsonPropertyName("totalPages")]
        [Required]
        public required int TotalPages { get; init; }

        /// <summary>
        /// A dictionary containing historical exchange rates, where the key is the date and the value is a dictionary of currency rates.
        /// </summary>
        [JsonPropertyName("rates")]
        [Required]
        public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; init; } = [];

        /// <summary>
        /// Creates a <see cref="GetHistoricalRatesResponseAppDto"/> from a service-level DTO.
        /// </summary>
        /// <param name="svcDto">The service-level DTO containing historical rates data.</param>
        /// <returns>A new instance of <see cref="GetHistoricalRatesResponseAppDto"/>.</returns>
        public static GetHistoricalRatesResponseAppDto FromSvcDto(CurrencyConverterHistoricalRatesPagedServiceResponseDto svcDto)
        {
            return new()
            {
                Base = svcDto.Base,
                Target = svcDto.Target,
                PageNumber = svcDto.PageNumber,
                PageSize = svcDto.PageSize,
                TotalItems = svcDto.TotalItems,
                TotalPages = svcDto.TotalPages,
                Rates = svcDto.Rates
            };
        }
    }
}