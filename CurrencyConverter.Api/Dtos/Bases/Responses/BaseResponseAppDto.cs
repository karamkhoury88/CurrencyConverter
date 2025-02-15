using System.Text.Json.Serialization;

namespace CurrencyConverter.Api.Dtos.Bases.Responses
{
    public record BaseResponseAppDTO
    {
        [JsonPropertyName("success")]
        public bool Success { get; init; } = true;
    }
}
