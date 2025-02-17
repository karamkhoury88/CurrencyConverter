using CurrencyConverter.ServiceDefaults.Constants;
using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Factories
{
    /// <summary>
    /// Factory implementation for creating instances of <see cref="ICurrencyConverterService"/> based on the provider name.
    /// </summary>
    public class CurrencyConverterFactory : ICurrencyConverterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrencyConverterFactory"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to resolve dependencies.</param>
        public CurrencyConverterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Retrieves an instance of <see cref="ICurrencyConverterService"/> for the specified provider.
        /// </summary>
        /// <param name="providerName">The name of the currency converter provider (e.g., "Frankfurter").</param>
        /// <returns>An instance of <see cref="ICurrencyConverterService"/> corresponding to the provider.</returns>
        /// <exception cref="AppException">
        /// Thrown when the specified provider is not supported.
        /// </exception>
        /// <remarks>
        /// This method uses the provider name to determine which implementation of <see cref="ICurrencyConverterService"/>
        /// to resolve from the dependency injection container. If the provider is not supported, an exception is thrown.
        /// </remarks>
        public ICurrencyConverterService GetConverter(string providerName)
        {
            // Use a switch expression to determine the correct service based on the provider name
            return providerName.ToLower() switch
            {
                // Resolve the FrankfurterCurrencyConverterService for the "Frankfurter" provider
                CurrencyConverterProviders.FRANKFURTER => _serviceProvider.GetRequiredService<ICurrencyConverterService>(),

                // Throw an exception for unsupported providers
                _ => throw new AppException(
                    errorCode: AppErrorCode.NOT_ALLOWED_OPERATION,
                    nonTechnicalMessage: $"The provider {providerName} is not supported, please contact the support for more information.",
                    technicalMessage: $"Provider {providerName} is not supported.")
            };
        }
    }
}