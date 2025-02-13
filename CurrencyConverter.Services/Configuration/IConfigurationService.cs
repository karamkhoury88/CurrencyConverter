using CurrencyConverter.Services.Configuration.Dtos;

namespace CurrencyConverter.Services.Configuration
{
    public interface IConfigurationService
    {
        CurrencyConverterConfigurationDto Config { get; }
    }
}
