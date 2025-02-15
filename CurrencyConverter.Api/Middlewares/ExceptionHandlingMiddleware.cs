using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
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
            ProblemDetails problemDetails = new();

            if (exception is AppException appException)
            {
                LogWarning(appException, httpContext);

                problemDetails.Detail = $"{(int)appException.ErrorCode}";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Type = "Bad Request";
                problemDetails.Title = appException.NonTechnicalMessage;
            }
            else
            {
                LogError(exception, httpContext);

                problemDetails.Detail = $"{StatusCodes.Status500InternalServerError}";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Type = "Internal Server Error";
                problemDetails.Title = $"Something went wrong, please try again.";
            }

            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            if (httpContext.Response.HasStarted)
            {
                logger.LogWarning("Response has already started. Cannot write ProblemDetails.");
                return false;
            }

           return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = exception,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
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

            return string.Join(" ", messages);
        }

        private void LogWarning(AppException appException, HttpContext httpContext)
        {
            var technicalMessage = appException.TechnicalMessage ?? appException.Message;
            var errorCode = appException.ErrorCode;
            string? activityId = GetActivityId(httpContext);
            var requestId = httpContext.TraceIdentifier;
            var fullExceptionMessage = GetFullExceptionMessage(appException);
            var exceptionData = JsonSerializer.Serialize(appException.Data);
            var stackTrace = appException.StackTrace;

            logger.LogWarning(
                "{TechnicalMessage} {ErrorCode} {ActivityId} {RequestId} {FullExceptionMessage} {ExceptionData} {StackTrace}",
                technicalMessage,
                errorCode,
                activityId,
                requestId,
                fullExceptionMessage,
                exceptionData,
                stackTrace
            );
        }

        private void LogError(Exception exception, HttpContext httpContext)
        {
            string? activityId = GetActivityId(httpContext);
            var requestId = httpContext.TraceIdentifier;
            var fullExceptionMessage = GetFullExceptionMessage(exception);
            var exceptionData = JsonSerializer.Serialize(exception.Data);
            var stackTrace = exception.StackTrace;

            logger.LogError(
                "{ActivityId} {RequestId} {FullExceptionMessage} {ExceptionData} {StackTrace}",
                activityId,
                requestId,
                fullExceptionMessage,
                exceptionData,
                stackTrace
            );
        }

        private static string? GetActivityId(HttpContext httpContext)
        {
            return (httpContext.Features.Get<IHttpActivityFeature>()?.Activity)?.Id;
        }

        #endregion
    }

    public class CustomProblemDetailsWriter : IProblemDetailsWriter
    {
        public bool CanWrite(ProblemDetailsContext context)
        {
            return context.HttpContext.Response.ContentType?.StartsWith("application/problem+json") == true;
        }

        public async ValueTask WriteAsync(ProblemDetailsContext context)
        {
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method}{context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            context.ProblemDetails.Extensions.TryAdd("activityId", (context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity)?.Id);

            var httpContext = context.HttpContext;

            httpContext.Response.StatusCode = context.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(context.ProblemDetails);
        }
    }
}
