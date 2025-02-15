using System.Text.Json.Serialization;
using System.Text.Json;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos
{
    public record CurrencyConverterHistoricalRatesServiceResponseDto
    {
        public required string Base { get; init; }
        public required Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; init; } = [];
    }

}
