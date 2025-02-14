using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Polly;
using System;
using System.Net;
using System.Text.Json;

namespace CurrencyConverter.Api.Middlewares
{
    public class ExceptionHandlingMiddleware(IProblemDetailsService problemDetailsService, ILogger<ExceptionHandlingMiddleware> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            ProblemDetails problem = new();
  
            if (exception is AppException appException)
            {
                LogWarning(appException);

                problem.Detail = $"{(int)appException.ErrorCode} : {appException.PublicMessage}";
                problem.Status = (int)HttpStatusCode.BadRequest;
                problem.Type = "Bad Request";
                problem.Title = "Error";

                httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                LogError(exception);

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

        #region Private

        private static string GetFullExceptionMessage(Exception ex)
        {
            if (ex == null) return string.Empty;

            var messages = new List<string>();
            var exceptions = new Queue<Exception>();
            exceptions.Enqueue(ex);

            while (exceptions.Count > 0)
            {
                var current = exceptions.Dequeue();
                messages.Add(current.Message);

                if (current is AggregateException aggEx)
                {
                    foreach (var inner in aggEx.InnerExceptions)
                    {
                        exceptions.Enqueue(inner);
                    }
                }
                else if (current.InnerException != null)
                {
                    exceptions.Enqueue(current.InnerException);
                }
            }

            return string.Join(" | ", messages);
        }

        private void LogWarning(AppException appException)
        {
            var technicalMessage = appException.TechnicalMessage ?? appException.Message;           
            var errorCode = appException.ErrorCode;
            var fullExceptionMessage = GetFullExceptionMessage(appException);
            var exceptionData = JsonSerializer.Serialize(appException.Data);
            var stackTrace = appException.StackTrace;

            logger.LogWarning(
                "{TechnicalMessage} | ErrorCode: {ErrorCode} | {FullExceptionMessage} | Data: {ExceptionData} | StackTrace: {StackTrace}",
                technicalMessage,
                errorCode,
                fullExceptionMessage, 
                exceptionData,
                stackTrace
            );
        }

        private void LogError(Exception exception)
        {

            var fullExceptionMessage = GetFullExceptionMessage(exception);
            var exceptionData = JsonSerializer.Serialize(exception.Data);
            var stackTrace = exception.StackTrace;

            logger.LogError(
                "{FullExceptionMessage} | Data: {ExceptionData} | StackTrace: {StackTrace}",
                fullExceptionMessage,
                exceptionData,
                stackTrace
            );
        }

        #endregion
    }
}
