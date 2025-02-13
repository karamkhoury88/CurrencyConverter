using CurrencyConverter.Services.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Services
{
    public static class CurrencyConverterServicesDiMapper
    {
        public static void MapAppServices(IServiceCollection serviceCollection)
        {
            #region ThirdParty Services


            #endregion

            #region App Services


            #endregion

            #region HelperServices

            // configuration service is added as a Singleton as we need one instance to be created in all application lifetime
            _ = serviceCollection.AddSingleton<IConfigurationService, ConfigurationService>();

            #endregion


        }
    }
}
