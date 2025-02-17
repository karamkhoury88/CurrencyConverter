using CurrencyConverter.Api.Common.Helpers;

namespace CurrencyConverter.Api.Middlewares
{
    /// <summary>
    /// Custom writer for ProblemDetails responses.
    /// Implements <see cref="IProblemDetailsWriter"/> to write ProblemDetails responses in a specific format.
    /// </summary>
    public class CustomProblemDetailsWriter : IProblemDetailsWriter
    {
        /// <summary>
        /// Determines whether this writer can handle the given ProblemDetails context.
        /// </summary>
        /// <param name="context">The ProblemDetails context.</param>
        /// <returns>True if the writer can handle the context; otherwise, false.</returns>
        public bool CanWrite(ProblemDetailsContext context)
        {
            // Only handle responses with content type "application/problem+json".
            return context.HttpContext.Response.ContentType?.StartsWith("application/problem+json") == true;
        }

        /// <summary>
        /// Writes the ProblemDetails response to the HTTP context.
        /// </summary>
        /// <param name="context">The ProblemDetails context.</param>
        /// <returns>A <see cref="ValueTask"/> representing the asynchronous operation.</returns>
        public async ValueTask WriteAsync(ProblemDetailsContext context)
        {
            // Add additional details to the ProblemDetails response.
            context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method}{context.HttpContext.Request.Path}";
            context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
            context.ProblemDetails.Extensions.TryAdd("activityId", HttpContextHelper.GetActivityId(context.HttpContext));

            // Write the ProblemDetails response as JSON.
            var httpContext = context.HttpContext;
            httpContext.Response.StatusCode = context.ProblemDetails.Status ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(context.ProblemDetails);
        }
    }
}