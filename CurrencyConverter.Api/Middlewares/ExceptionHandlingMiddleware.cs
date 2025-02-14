using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Trace;
using System.Net;

namespace CurrencyConverter.Api.Middlewares
{
    public class ExceptionHandlingMiddleware(IProblemDetailsService problemDetailsService) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            ProblemDetails problem = new();

            if (exception is AppException appException)
            {

                problem.Detail = $"{(int)appException.ErrorCode} : {appException.PublicMessage}";
                problem.Status = (int)HttpStatusCode.BadRequest;
                problem.Type = "Bad Request";
                problem.Title = "Error";

                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                problem.Detail = $"Something went wrong, please try again.";
                problem.Status = (int)HttpStatusCode.InternalServerError;
                problem.Type = "Internal Server Error";
                problem.Title = "Exception";

                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            ProblemDetailsContext errorContext = new()
            {

                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = problem
            };

            await problemDetailsService.WriteAsync(errorContext);
            return true;
        }
    }
}
