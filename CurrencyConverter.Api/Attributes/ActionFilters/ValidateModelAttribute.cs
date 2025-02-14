using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using CurrencyConverter.Api.Common.Exceptions;

namespace CurrencyConverter.Api.Attributes.ActionFilters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if ((context.HttpContext.Request.Method == HttpMethods.Post ||
                context.HttpContext.Request.Method == HttpMethods.Put) 
                && !context.ModelState.IsValid)
            {

                throw new ApiModelValidationException(context.ModelState);
            }
        }
    }
}
