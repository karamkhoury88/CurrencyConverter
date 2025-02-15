using CurrencyConverter.Services.AppServices.Configuration;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter;
using CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Factories;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Services
{
    public static class CurrencyConverterServicesDiMapper
    {
        public static void MapAppServices(IServiceCollection serviceCollection)
        {
            #region ThirdParty Services

            serviceCollection.AddScoped<FrankfurterCurrencyConverter>();
            serviceCollection.AddScoped<ICurrencyConverterFactory, CurrencyConverterFactory>();

            #endregion

            #region HelperServices

            // configuration service is added as a Singleton as we need one instance to be created in all application lifetime
            _ = serviceCollection.AddSingleton<IConfigurationService, ConfigurationService>();

            #endregion
        }
    }
}
