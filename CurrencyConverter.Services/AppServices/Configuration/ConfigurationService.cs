using CurrencyConverter.Services.AppServices.Configuration.Dtos;
using Microsoft.Extensions.Configuration;

namespace CurrencyConverter.Services.AppServices.Configuration
{
    /// <summary>
    /// Implements the <see cref="IConfigurationService"/> interface to provide centralized access to application configuration.
    /// This service binds configuration sections to strongly-typed objects and ensures their validity.
    /// </summary>
    internal class ConfigurationService : IConfigurationService
    {
        /// <summary>
        /// Gets the application configuration as a strongly-typed object.
        /// </summary>
        public CurrencyConverterConfigurationDto Config { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
        /// </summary>
        /// <param name="configuration">The application configuration provider.</param>
        /// <exception cref="InvalidOperationException">Thrown if the required configuration section is missing or invalid.</exception>
        public ConfigurationService(IConfiguration configuration)
        {
            // Bind the "CurrencyConverterConfiguration" section to a strongly-typed object.
            CurrencyConverterConfigurationDto? currencyConverterConfiguration = new CurrencyConverterConfigurationDto();
            configuration.GetSection("CurrencyConverterConfiguration").Bind(currencyConverterConfiguration);

            // Ensure the configuration section is not null.
            if (currencyConverterConfiguration == null)
            {
                throw new InvalidOperationException("CurrencyConverterConfiguration section is missing or invalid.");
            }

            // Validate the configuration to ensure all required values are present and valid.
            currencyConverterConfiguration.Validate();

            // Assign the validated configuration to the Config property.
            Config = currencyConverterConfiguration;
        }
    }
}