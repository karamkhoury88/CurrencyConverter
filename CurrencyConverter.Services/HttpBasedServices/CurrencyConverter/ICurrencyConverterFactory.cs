using CurrencyConverter.ServiceDefaults.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CurrencyConverter.Services.HttpBasedServices.CurrencyConverter
{
    public interface ICurrencyConverterFactory
    {
        ICurrencyConverterService GetConverter(string providerName = CurrencyConverterProviders.FRANKFURTER);
    }
}
