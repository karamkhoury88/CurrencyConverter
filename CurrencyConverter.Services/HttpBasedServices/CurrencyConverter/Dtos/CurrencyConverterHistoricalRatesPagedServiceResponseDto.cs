namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos
{
    public record CurrencyConverterHistoricalRatesPagedServiceResponseDto
    {
        public required string Base { get; init; }
        public required string Target { get; init; }
        public required int PageNumber { get; init; }
        public required int PageSize { get; init; }
        public required int TotalItems { get; init; }
        public required int TotalPages { get; init; }
        public Dictionary<DateTime, Dictionary<string, decimal>> Rates { get; init; } = [];
    }
}
