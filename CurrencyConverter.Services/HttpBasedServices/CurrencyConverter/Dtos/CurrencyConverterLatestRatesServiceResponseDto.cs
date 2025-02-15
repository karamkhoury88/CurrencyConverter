namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos
{
    public record CurrencyConverterLatestRatesServiceResponseDto
    {
        public required string Base { get; init; }    // The base currency (e.g., "EUR")
        public required DateTime Date { get; init; }  // The date of the exchange rates
        public required Dictionary<string, decimal> Rates { get; init; } = [];
    }
}
