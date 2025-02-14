using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CurrencyConverter.Api.Common.Exceptions
{
    internal class ApiModelValidationException : AppException
    {
        public ApiModelValidationException(ModelStateDictionary model): base(AppErrorCode.INVALID_PARAMETER)
        {
            PublicMessage = string.Join(" | ", model.Values.SelectMany(a => a.Errors.Select(b => b.ErrorMessage)).ToList());
        }
    }
}
