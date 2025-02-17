using CurrencyConverter.Services.AppServices.Configuration.Dtos;

namespace CurrencyConverter.Services.AppServices.Configuration
{
    /// <summary>
    /// Provides a centralized service for accessing application configuration in a structured way.
    /// This service avoids relying on string-based keys and instead exposes configuration as strongly-typed objects.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the application configuration as a strongly-typed object.
        /// </summary>
        CurrencyConverterConfigurationDto Config { get; }
    }
}