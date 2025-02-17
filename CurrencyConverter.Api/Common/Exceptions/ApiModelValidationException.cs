using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CurrencyConverter.Api.Common.Exceptions
{
    /// <summary>
    /// Represents an exception thrown when model validation fails in the API.
    /// Inherits from <see cref="AppException"/> to provide custom error handling.
    /// </summary>
    internal class ApiModelValidationException : AppException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiModelValidationException"/> class.
        /// </summary>
        /// <param name="model">The <see cref="ModelStateDictionary"/> containing validation errors.</param>
        public ApiModelValidationException(ModelStateDictionary model)
            : base(AppErrorCode.INVALID_PARAMETER, nonTechnicalMessage: "", technicalMessage: "The request model is not valid.")
        {
            // Combine all validation error messages into a single non-technical message.
            NonTechnicalMessage = string.Join(" ",
                model.Values.SelectMany(a => a.Errors.Select(b => b.ErrorMessage)).ToList());
        }
    }
}