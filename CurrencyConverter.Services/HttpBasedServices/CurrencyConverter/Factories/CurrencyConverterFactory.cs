using CurrencyConverter.ServiceDefaults.Constants;
using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter.Factories
{
    public class CurrencyConverterFactory : ICurrencyConverterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyConverterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICurrencyConverterService GetConverter(string providerName)
        {
            return providerName.ToLower() switch
            {
                CurrencyConverterProviders.FRANKFURTER => _serviceProvider.GetRequiredService<FrankfurterCurrencyConverter>(),
                _ => throw new AppException(errorCode: AppErrorCode.NOT_ALLOWED_OPERATION, nonTechnicalMessage: $"The provider {providerName} is not supported, please contact the support for more information.", technicalMessage: $"Provider {providerName} is not supported.")
            };
        }
    }
}
