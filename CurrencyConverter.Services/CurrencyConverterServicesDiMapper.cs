using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.AppServices.CustomizedHybridCache;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Factories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace CurrencyConverter.Services
{
    /// <summary>
    /// Static class responsible for mapping and registering services in the dependency injection (DI) container.
    /// </summary>
    public static class CurrencyConverterServicesDiMapper
    {
        /// <summary>
        /// Maps and registers application services in the DI container.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add services to.</param>
        public static void MapAppServices(IServiceCollection serviceCollection)
        {
           

            #region HelperServices

            // Register the ConfigurationService as a singleton.
            // Singleton services are created once and shared throughout the application's lifetime.
            // This is ideal for configuration services as they typically do not change during runtime.
            _ = serviceCollection.AddSingleton<IConfigurationService, ConfigurationService>();

            // Register the CustomizedHybridCacheService as a singleton.
            // This service provides a hybrid caching mechanism (in-memory and distributed cache).
            // Singleton ensures a single instance is shared across the application.
            _ = serviceCollection.AddSingleton<ICustomizedHybridCacheService, CustomizedHybridCacheService>();

            #endregion

            #region ThirdParty Services

            // Register the FrankfurterCurrencyConverterService as a scoped service.
            // Scoped services are created once per client request.
            serviceCollection.AddScoped<ICurrencyConverterService, FrankfurterCurrencyConverterService>();

            // Register the CurrencyConverterFactory as a scoped service.
            // This factory is responsible for creating instances of currency converter services.
            serviceCollection.AddScoped<ICurrencyConverterFactory, CurrencyConverterFactory>();

            #endregion
        }
    }
}