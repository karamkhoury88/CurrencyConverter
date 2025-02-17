using CurrencyConverter.Api.Dtos.Bases.Responses;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.CurrencyConverter.Responses
{
    /// <summary>
    /// Represents the data transfer object for a currency conversion response.
    /// </summary>
    public record ConvertResponseAppDto : BaseResponseAppDTO
    {
        /// <summary>
        /// The amount in the base currency to be converted.
        /// </summary>
        [JsonPropertyName("baseAmount")]
        [Required]
        public required decimal BaseAmount { get; init; }

        /// <summary>
        /// The currency code of the base amount.
        /// </summary>
        [JsonPropertyName("baseCurrency")]
        [Required]
        public required string BaseCurrency { get; init; }

        /// <summary>
        /// The converted amount in the target currency.
        /// </summary>
        [JsonPropertyName("targetAmount")]
        [Required]
        public decimal TargetAmount { get; set; }

        /// <summary>
        /// The currency code of the target amount.
        /// </summary>
        [JsonPropertyName("targetCurrency")]
        [Required]
        public required string TargetCurrency { get; init; }

        /// <summary>
        /// Creates a <see cref="ConvertResponseAppDto"/> from a service-level DTO.
        /// </summary>
        /// <param name="svcDto">The service-level DTO containing conversion details.</param>
        /// <returns>A new instance of <see cref="ConvertResponseAppDto"/>.</returns>
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