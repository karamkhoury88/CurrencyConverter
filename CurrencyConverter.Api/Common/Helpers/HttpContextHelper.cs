using Microsoft.AspNetCore.Http.Features;

namespace CurrencyConverter.Api.Common.Helpers
{
    /// <summary>
    /// Provides helper methods for working with <see cref="HttpContext"/>.
    /// </summary>
    public class HttpContextHelper
    {
        /// <summary>
        /// Retrieves the activity ID associated with the current HTTP request.
        /// The activity ID is used for distributed tracing and logging purposes.
        /// </summary>
        /// <param name="httpContext">The HTTP context for the current request.</param>
        /// <returns>
        /// The activity ID as a string if available; otherwise, <c>null</c>.
        /// </returns>
        public static string? GetActivityId(HttpContext httpContext)
        {
            // Retrieve the IHttpActivityFeature from the HTTP context features.
            // This feature provides access to the activity associated with the request.
            var activityFeature = httpContext.Features.Get<IHttpActivityFeature>();

            // Return the activity ID if the activity feature and activity are available.
            return activityFeature?.Activity?.Id;
        }
    }
}