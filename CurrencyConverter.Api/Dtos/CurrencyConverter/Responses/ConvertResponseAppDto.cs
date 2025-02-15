using CurrencyConverter.Api.Dtos.Bases.Responses;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.CurrencyConverter.Responses
{
    public record ConvertResponseAppDto : BaseResponseAppDTO
    {
        [JsonPropertyName("baseAmount")]
        [Required]
        public required decimal BaseAmount { get; init; }

        [JsonPropertyName("baseCurrency")]
        [Required]
        public required string BaseCurrency { get; init; }

        [JsonPropertyName("targetAmount")]
        [Required]
        public decimal TargetAmount { get; set; }

        [JsonPropertyName("targetCurrency")]
        [Required]
        public required string TargetCurrency { get; init; }

        public static ConvertResponseAppDto FromSvcDto(CurrencyConverterConvertingServiceResponseDto svcDto)
        {
            return new()
            {
                BaseAmount = svcDto.BaseAmount,
                BaseCurrency = svcDto.BaseCurrency,
                TargetCurrency = svcDto.TargetCurrency,
                TargetAmount = svcDto.TargetAmount,
            };
        }
    }
}
