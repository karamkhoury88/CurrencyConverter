using Microsoft.Extensions.Logging;

namespace CurrencyConverter.ServiceDefaults.Handlers
{
    /// <summary>
    /// A custom HTTP message handler for logging HTTP requests and responses.
    /// </summary>
    internal class HttpMessageLoggingHandler(ILogger<HttpMessageLoggingHandler> logger) : DelegatingHandler
    {
        /// <summary>
        /// Sends an HTTP request and logs the request and response details.
        /// </summary>
        /// <param name="request">The HTTP request message to send.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>The HTTP response message.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Log the HTTP request details (method and URI).
            logger.LogInformation("Sending HTTP request: {Method} {Uri}", request.Method, request.RequestUri);

            // Send the HTTP request and await the response.
            var response = await base.SendAsync(request, cancellationToken);

            // Log the HTTP response status code.
            logger.LogInformation("Received HTTP response: {StatusCode}", response.StatusCode);

            // Return the HTTP response.
            return response;
        }
    }
}