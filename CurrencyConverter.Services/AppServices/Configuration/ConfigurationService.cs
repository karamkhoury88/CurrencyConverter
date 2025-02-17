using System.Runtime.CompilerServices;
using CurrencyConverter.Services.AppServices.Configuration.Dtos;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("CurrencyConverter.Tests")]
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
            var section = configuration.GetSection("CurrencyConverterConfiguration");
            var currencyConverterConfiguration = new CurrencyConverterConfigurationDto();
            Bind(section, currencyConverterConfiguration);

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

        /// <summary>
        /// Binds the configuration section to the specified instance.
        /// </summary>
        /// <typeparam name="T">The type of the instance to bind to.</typeparam>
        /// <param name="section">The configuration section containing the data to bind.</param>
        /// <param name="instance">The instance of the type to bind the configuration data to.</param>
        private static void Bind<T>(IConfigurationSection section, T instance)
        {
            // The Bind method binds the configuration data from the specified section to the given instance.
            // This extension method is provided by the Microsoft.Extensions.Configuration.Binder namespace.
            section.Bind(instance);
        }

    }
}