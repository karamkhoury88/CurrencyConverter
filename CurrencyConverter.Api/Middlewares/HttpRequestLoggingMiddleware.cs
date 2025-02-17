using System.Security.Claims;

namespace CurrencyConverter.Api.Middlewares
{
    /// <summary>
    /// Middleware for logging HTTP request details, including method, path, response time, client ID, and IP address.
    /// </summary>
    public class HttpRequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpRequestLoggingMiddleware> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRequestLoggingMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="logger">The logger used to log request details.</param>
        public HttpRequestLoggingMiddleware(RequestDelegate next, ILogger<HttpRequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Invokes the middleware to log HTTP request details.
        /// </summary>
        /// <param name="context">The HTTP context for the current request.</param>
        /// <param name="logger">The logger instance (injected by dependency injection).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task Invoke(HttpContext context, ILogger<HttpRequestLoggingMiddleware> logger)
        {
            // Capture the start time of the request.
            var startTime = DateTime.UtcNow;

            // Call the next middleware in the pipeline.
            await _next(context);

            // Calculate the elapsed time for the request.
            var elapsed = DateTime.UtcNow - startTime;

            // Extract the ClientId from the JWT token (if available).
            var clientId = context.User.FindFirstValue("client_id") ?? "Anonymous";

            // Extract the Client IP address.
            var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Log the request details using structured logging.
            _logger.LogInformation("HTTP {Method} {Path} handled in {Elapsed} ms by {ClientId} (IP: {ClientIp}) with status {StatusCode}",
                context.Request.Method, // HTTP Method (e.g., GET, POST)
                context.Request.Path,    // Target Endpoint (e.g., /api/users)
                elapsed.TotalMilliseconds, // Response Time in milliseconds
                clientId,                // ClientId from JWT token
                clientIp,                // Client IP address
                context.Response.StatusCode); // Response Code
        }
    }
}