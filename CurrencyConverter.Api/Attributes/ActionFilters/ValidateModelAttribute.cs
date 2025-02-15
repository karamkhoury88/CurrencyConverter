using CurrencyConverter.Api.Common.Exceptions;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CurrencyConverter.Api.Attributes.ActionFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {

                throw new ApiModelValidationException(context.ModelState);
            }
        }
    }
}
