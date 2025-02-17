using CurrencyConverter.ServiceDefaults.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Factories
{
    /// <summary>
    /// Defines a factory interface for creating instances of <see cref="ICurrencyConverterService"/> based on the provider name.
    /// </summary>
    public interface ICurrencyConverterFactory
    {
        /// <summary>
        /// Retrieves an instance of <see cref="ICurrencyConverterService"/> for the specified provider.
        /// </summary>
        /// <param name="providerName">The name of the currency converter provider (e.g., "Frankfurter").</param>
        /// <returns>An instance of <see cref="ICurrencyConverterService"/> corresponding to the provider.</returns>
        /// <remarks>
        /// This method allows the application to dynamically select and use different currency converter providers
        /// based on configuration or runtime requirements.
        /// </remarks>
        ICurrencyConverterService GetConverter(string providerName);
    }
}