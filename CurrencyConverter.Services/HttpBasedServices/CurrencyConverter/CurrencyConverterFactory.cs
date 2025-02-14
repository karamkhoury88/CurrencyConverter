using CurrencyConverter.ServiceDefaults.Constants;
using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter
{
    public class CurrencyConverterFactory : ICurrencyConverterFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CurrencyConverterFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ICurrencyConverterService GetConverter(string providerName = "Frankfurter")
        {
            return providerName switch
            {
                CurrencyConverterProviders.FRANKFURTER => _serviceProvider.GetRequiredService<FrankfurterCurrencyConverter>(),
                _ => throw new AppException(errorCode: AppErrorCode.NOT_ALLOWED_OPERATION, $"Provider {providerName} is not supported.")
            };
        }
    }
}
