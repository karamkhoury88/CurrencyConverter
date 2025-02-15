using CurrencyConverter.Api.Dtos.Bases.Responses;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.CurrencyConverter.Responses
{
    public record GetHistoricalRatesResponseAppDto : BaseResponseAppDTO
    {
        [JsonPropertyName("base")]
        [Required]
        public required string Base { get; init; }

        [JsonPropertyName("target")]
        [Required]
        public required string Target { get; init; }

        [JsonPropertyName("pageNumber")]
        [Required]
        public required int PageNumber { get; init; }

        [JsonPropertyName("pageSize")]
        [Required]
        public required int PageSize { get; init; }

        [JsonPropertyName("totalItems")]
        [Required]
        public required int TotalItems { get; init; }

        [JsonPropertyName("totalPages")]
        [Required]
        public required int TotalPages { get; init; }

        [JsonPropertyName("rates")]
        [Required]
        public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; init; } = [];

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
