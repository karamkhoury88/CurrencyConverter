using CurrencyConverter.ServiceDefaults.Handlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
/// This project should be referenced by each service project in your solution.
/// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds default services for .NET Aspire applications, including OpenTelemetry, health checks, service discovery, and resilience.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Configure OpenTelemetry for logging, metrics, and tracing.
        builder.ConfigureOpenTelemetry();

        // Add default health checks.
        builder.AddDefaultHealthChecks();

        #region Builder Services Default

        // Enable service discovery for resolving service endpoints.
        builder.Services.AddServiceDiscovery();

        // Configure rate limiting to prevent abuse of the API.
        // TODO: Update the README to mention rate limiting configuration.
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    // Use user ID for authenticated users.
                    var userKey = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown-user";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 200, // Allow 200 requests per minute for authenticated users.
                            Window = TimeSpan.FromMinutes(1)
                        });
                }
                else
                {
                    // Use IP address for non-authenticated users.
                    var ipKey = httpContext.Request.Headers["X-Forwarded-For"].ToString()
                                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown-ip";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ipKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100, // Allow 100 requests per minute for non-authenticated users.
                            Window = TimeSpan.FromMinutes(1)
                        });
                }
            });
        });

        #endregion

        #region Http Clients Defaults

        // Register the HTTP message logging handler.
        builder.Services.AddTransient<HttpMessageLoggingHandler>();

        // Register IHttpClientFactory and HttpClient.
        builder.Services.AddHttpClient();

        // Configure default settings for all HttpClient instances.
        builder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
        {
            // Enable resilience by default (retry, timeout, circuit breaker, rate limiting).
            // https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience?tabs=dotnet-cli
            httpClientBuilder.AddStandardResilienceHandler();

            // Enable service discovery for resolving service endpoints.
            httpClientBuilder.AddServiceDiscovery();

            // Add the HTTP message logging handler to all HttpClient instances.
            httpClientBuilder.AddHttpMessageHandler<HttpMessageLoggingHandler>();

            // Configure default headers for all HttpClient instances.
            httpClientBuilder.ConfigureHttpClient(client =>
            {
                // Add a correlation ID to track requests across services.
                var correlationId = Guid.NewGuid().ToString();
                client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);
            });
        });

        #endregion

        return builder;
    }

    /// <summary>
    /// Adds default health checks to the application.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Add a default health check to ensure the application is responsive.
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Maps default endpoints for health checks in development environments.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // Map a health check endpoint to ensure the application is ready to accept traffic.
            app.MapHealthChecks("/health");

            // Map a liveness endpoint to ensure the application is alive.
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    #region Privates

    /// <summary>
    /// Configures OpenTelemetry for logging, metrics, and tracing.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The host application builder for chaining.</returns>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Configure OpenTelemetry logging.
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true; // Include formatted log messages.
            logging.IncludeScopes = true; // Include log scopes (additional context).
            logging.ParseStateValues = true; // Parse and include structured log state values.
        });

        // Configure OpenTelemetry metrics and tracing.
        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation() // Instrument ASP.NET Core metrics.
                    .AddHttpClientInstrumentation() // Instrument HttpClient metrics.
                    .AddRuntimeInstrumentation(); // Instrument .NET runtime metrics.
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName) // Add the application name as a trace source.
                    .AddAspNetCoreInstrumentation() // Instrument ASP.NET Core tracing.
                    .AddHttpClientInstrumentation(); // Instrument HttpClient tracing.
            });

        // Add OpenTelemetry exporters (e.g., OTLP, Azure Monitor).
        builder.AddOpenTelemetryExporters();

        return builder;
    }

    /// <summary>
    /// Adds OpenTelemetry exporters based on configuration.
    /// </summary>
    /// <param name="builder">The host application builder.</param>
    /// <returns>The host application builder for chaining.</returns>
    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        // Check if the OTLP exporter endpoint is configured.
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            // Use the OTLP exporter if configured.
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package).
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    #endregion
}