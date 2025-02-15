namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos
{
    public record CurrencyConverterConvertingServiceResponseDto
    {
        public required decimal BaseAmount { get; init; }
        public required string BaseCurrency { get; init; }
        public decimal TargetAmount { get; set; }
        public required string TargetCurrency { get; init; }
    }
}
