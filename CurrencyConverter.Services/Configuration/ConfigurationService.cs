using CurrencyConverter.Services.Configuration.Dtos;
using Microsoft.Extensions.Configuration;
using System.Runtime;
using System.Text.Json;

namespace CurrencyConverter.Services.Configuration
{
    internal class ConfigurationService : IConfigurationService
    {
        public CurrencyConverterConfigurationDto Config { get; }

        public ConfigurationService(IConfiguration configuration)
        {

            // Bind the configuration section to the complex object
            CurrencyConverterConfigurationDto? currencyConverterConfiguration = new CurrencyConverterConfigurationDto();
            configuration.GetSection("CurrencyConverterConfiguration").Bind(currencyConverterConfiguration);

            if (currencyConverterConfiguration == null)
            {
                throw new InvalidOperationException("Configuration are missing");
            }

            currencyConverterConfiguration.Validate();

            Config = currencyConverterConfiguration;

        }

    }
}