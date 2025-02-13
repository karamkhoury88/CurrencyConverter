using System.Security.Claims;

namespace CurrencyConverter.Api.Middlewares
{
    public class HttpRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpRequestLoggingMiddleware> _logger;

        public HttpRequestLoggingMiddleware(RequestDelegate next, ILogger<HttpRequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context, ILogger<HttpRequestLoggingMiddleware> logger)
        {
            // Capture the start time of the request
            var startTime = DateTime.UtcNow;

            // Call the next middleware in the pipeline
            await _next(context);

            // Calculate the elapsed time for the request
            var elapsed = DateTime.UtcNow - startTime;

            // Extract the ClientId from the JWT token (if available)
            var clientId = context.User.FindFirstValue("client_id") ?? "Anonymous";

            // Extract the Client IP address
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Log the request details using structured logging
            _logger.LogInformation("HTTP {Method} {Path} handled in {Elapsed} ms by {ClientId} (IP: {ClientIp}) with status {StatusCode}",
                context.Request.Method, // HTTP Method (e.g., GET, POST)
                context.Request.Path,    // Target Endpoint (e.g., /api/users)
                elapsed.TotalMilliseconds, // Response Time in milliseconds
                clientId,                // ClientId from JWT token
                clientIp,                // Client IP address
                context.Response.StatusCode); // Response Code (e.g., 200, 404)
        }
    }
}
