using CurrencyConverter.Services.AppServices.Configuration.Dtos;

namespace CurrencyConverter.Services.AppServices.Configuration
{
    public interface IConfigurationService
    {
        CurrencyConverterConfigurationDto Config { get; }
    }
}
