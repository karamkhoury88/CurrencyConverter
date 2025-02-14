using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Dtos
{
    public record ConvertCurrencyServiceResponseDto
    {
        [JsonPropertyName("fromAmount")]
        [Required]
        public decimal FromAmount { get; set; }

        [JsonPropertyName("fromCurrency")]
        [Required]
        public string FromCurrency { get; set; }


        [JsonPropertyName("toAmount")]
        [Required]
        public decimal ToAmount { get; set; }

        [JsonPropertyName("fromCurrency")]
        [Required]
        public string ToCurrency { get; set; }
    }
}
