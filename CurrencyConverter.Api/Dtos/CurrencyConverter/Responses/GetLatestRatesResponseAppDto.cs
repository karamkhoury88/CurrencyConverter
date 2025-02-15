using CurrencyConverter.Api.Dtos.Bases.Responses;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.CurrencyConverter.Responses
{
    public record GetLatestRatesResponseAppDto : BaseResponseAppDTO
    {
        [JsonPropertyName("base")]
        [Required]
        public required string Base { get; init; }    // The base currency (e.g., "EUR")

        [JsonPropertyName("date")]
        [Required]
        public required DateTime Date { get; init; }  // The date of the exchange rates

        [JsonPropertyName("rates")]
        [Required]
        public required Dictionary<string, decimal> Rates { get; init; } = [];

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
