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

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        #region Builder Services Default

        builder.Services.AddServiceDiscovery();

        // Configure Rate Limiting
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                if (httpContext.User?.Identity?.IsAuthenticated == true)
                {
                    // Use user ID for authenticated users
                    var userKey = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown-user";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: userKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 15,
                            Window = TimeSpan.FromMinutes(1)
                        });
                }
                else
                {
                    // Use IP address for non-authenticated users
                    var ipKey = httpContext.Request.Headers["X-Forwarded-For"].ToString()
                                ?? httpContext.Connection.RemoteIpAddress?.ToString()
                                ?? "unknown-ip";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ipKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(1)
                        });
                }
            });
        });

        #endregion

        #region Http Clients Defaults

        builder.Services.AddTransient<HttpMessageLoggingHandler>();

        // Registers IHttpClientFactory and HttpClient
        builder.Services.AddHttpClient();

        builder.Services.ConfigureHttpClientDefaults(httpClientBuilder =>
        {
            // Turn on resilience by default:
            /*
             Key Features of AddStandardResilienceHandler
             Retry Policy:

                 - Automatically retries failed requests with a backoff strategy (e.g., exponential backoff).
                 - Helps handle transient errors like network glitches or temporary server unavailability.

             Timeout Policy:

                - Ensures that requests do not hang indefinitely by applying a timeout.

             Circuit Breaker:

                - Prevents overwhelming a failing service by temporarily stopping requests if a certain threshold of failures is reached.

             Rate Limiting:

                - Limits the number of requests sent to a service to avoid overloading it.

            Defaults:
            1 - Rate limiter:
            The rate limiter pipeline limits the maximum number of concurrent requests being sent to the dependency.	
            Queue: 0
			Permit: 1_000


            2 - Total timeout:
            The total request timeout pipeline applies an overall timeout to the execution, ensuring that the request, including retry attempts, doesn't exceed the configured limit.	
            Total timeout: 30s
           
            3 - Retry:
            The retry pipeline retries the request in case the dependency is slow or returns a transient error.
            Max retries: 3
			Backoff: Exponential
			Use jitter: true
			Delay:2s

            4 - Circuit breaker:	
            The circuit breaker blocks the execution if too many direct failures or timeouts are detected.	
            Failure ratio: 10%
			Min throughput: 100
			Sampling duration: 30s
			Break duration: 5s

           5 - Attempt timeout:
            The attempt timeout pipeline limits each request attempt duration and throws if it's exceeded.	
            Attempt timeout: 10s


            */
            httpClientBuilder.AddStandardResilienceHandler();

            // Turn on service discovery by default
            httpClientBuilder.AddServiceDiscovery();

            // Configure HttpClient with logging

            httpClientBuilder.AddHttpMessageHandler<HttpMessageLoggingHandler>();

            // Configure Http Clients defaults
            httpClientBuilder.ConfigureHttpClient(client =>
            {
                // Generate or retrieve a correlation ID
                var correlationId = Guid.NewGuid().ToString();
                client.DefaultRequestHeaders.Add("X-Correlation-ID", correlationId);

            });
        });

        #endregion

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
        // Add a default liveness check to ensure app is responsive
        .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks("/health");

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks("/alive", new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    #region Privates

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            // Include the formatted message in the log records
            logging.IncludeFormattedMessage = true;

            // Include log scopes (additional context) in the log records
            logging.IncludeScopes = true;

            // Parse and include structured log state values in the log records
            logging.ParseStateValues = true;
        });

        builder.Services.AddOpenTelemetry()
        .WithMetrics(metrics =>
        {
            metrics.AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation();
        })
        .WithTracing(tracing =>
        {
            tracing.AddSource(builder.Environment.ApplicationName)
                .AddAspNetCoreInstrumentation()
                // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                //.AddGrpcClientInstrumentation()
                .AddHttpClientInstrumentation();
        });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        // Uncomment the following lines to enable the Azure Monitor exporter (requires the Azure.Monitor.OpenTelemetry.AspNetCore package)
        //if (!string.IsNullOrEmpty(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]))
        //{
        //    builder.Services.AddOpenTelemetry()
        //       .UseAzureMonitor();
        //}

        return builder;
    }

    #endregion
}
