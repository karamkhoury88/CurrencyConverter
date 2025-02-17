using CurrencyConverter.Api.Common.Helpers;
using CurrencyConverter.ServiceDefaults.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace CurrencyConverter.Api.Middlewares
{
    /// <summary>
    /// Middleware for handling exceptions globally in the application.
    /// Implements <see cref="IExceptionHandler"/> to process exceptions and return appropriate ProblemDetails responses.
    /// </summary>
    public class ExceptionHandlingMiddleware(IProblemDetailsService problemDetailsService, ILogger<ExceptionHandlingMiddleware> logger) : IExceptionHandler
    {
        /// <summary>
        /// Handles exceptions and generates a ProblemDetails response.
        /// </summary>
        /// <param name="httpContext">The HTTP context where the exception occurred.</param>
        /// <param name="exception">The exception that was thrown.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A <see cref="ValueTask{TResult}"/> representing the result of the operation.</returns>
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // Initialize a ProblemDetails object to store error details.
            ProblemDetails problemDetails = new();

            // Check if the exception is of type AppException (custom application exception).
            if (exception is AppException appException)
            {
                // Log the exception as a warning since it's a known application error.
                LogWarning(appException, httpContext);

                // Populate ProblemDetails with custom error information.
                problemDetails.Detail = $"{(int)appException.ErrorCode}";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Type = "Bad Request";
                problemDetails.Title = appException.NonTechnicalMessage;
            }
            else
            {
                // Log the exception as an error since it's an unexpected or system-level error.
                LogError(exception, httpContext);

                // Populate ProblemDetails with generic error information.
                problemDetails.Detail = $"{StatusCodes.Status500InternalServerError}";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Type = "Internal Server Error";
                problemDetails.Title = $"Something went wrong, please try again.";
            }

            // Set the HTTP response status code and content type.
            httpContext.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;
            httpContext.Response.ContentType = "application/problem+json";

            // Check if the response has already started. If so, we cannot modify it further.
            if (httpContext.Response.HasStarted)
            {
                logger.LogWarning("Response has already started. Cannot write ProblemDetails.");
                return false;
            }

            // Write the ProblemDetails response using the ProblemDetailsService.
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                Exception = exception,
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });
        }

        #region Private

        /// <summary>
        /// Recursively retrieves the full exception message, including inner exceptions.
        /// </summary>
        /// <param name="ex">The exception to process.</param>
        /// <returns>A string containing the full exception message.</returns>
        private static string GetFullExceptionMessage(Exception ex)
        {
            if (ex == null) return string.Empty;

            var messages = new List<string>();
            var exceptions = new Queue<Exception>();
            exceptions.Enqueue(ex);

            // Traverse the exception hierarchy to collect all messages.
            while (exceptions.Count > 0)
            {
                var current = exceptions.Dequeue();
                messages.Add(current.Message);

                // Handle AggregateException (multiple inner exceptions).
                if (current is AggregateException aggEx)
                {
                    foreach (var inner in aggEx.InnerExceptions)
                    {
                        exceptions.Enqueue(inner);
                    }
                }
                // Handle single inner exception.
                else if (current.InnerException != null)
                {
                    exceptions.Enqueue(current.InnerException);
                }
            }

            // Combine all messages into a single string.
            return string.Join(" ", messages);
        }

        /// <summary>
        /// Logs an AppException as a warning with detailed information.
        /// </summary>
        /// <param name="appException">The custom application exception to log.</param>
        /// <param name="httpContext">The HTTP context where the exception occurred.</param>
        private void LogWarning(AppException appException, HttpContext httpContext)
        {
            // Extract technical details from the exception.
            var technicalMessage = appException.TechnicalMessage ?? appException.Message;
            var errorCode = appException.ErrorCode;
            string? activityId = HttpContextHelper.GetActivityId(httpContext);
            var requestId = httpContext.TraceIdentifier;
            var fullExceptionMessage = GetFullExceptionMessage(appException);
            var exceptionData = JsonSerializer.Serialize(appException.Data);
            var stackTrace = appException.StackTrace;

            // Log the exception as a warning with structured data.
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

        /// <summary>
        /// Logs a generic exception as an error with detailed information.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        /// <param name="httpContext">The HTTP context where the exception occurred.</param>
        private void LogError(Exception exception, HttpContext httpContext)
        {
            // Extract technical details from the exception.
            string? activityId = HttpContextHelper.GetActivityId(httpContext);
            var requestId = httpContext.TraceIdentifier;
            var fullExceptionMessage = GetFullExceptionMessage(exception);
            var exceptionData = JsonSerializer.Serialize(exception.Data);
            var stackTrace = exception.StackTrace;

            // Log the exception as an error with structured data.
            logger.LogError(
                "{ActivityId} {RequestId} {FullExceptionMessage} {ExceptionData} {StackTrace}",
                activityId,
                requestId,
                fullExceptionMessage,
                exceptionData,
                stackTrace
            );
        }

        #endregion
    }
}