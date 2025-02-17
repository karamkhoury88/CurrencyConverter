using CurrencyConverter.Api.Common.Helpers;
using CurrencyConverter.ServiceDefaults.Exceptions;
using CurrencyConverter.Services.AppServices.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CurrencyConverter.Api.Middlewares
{
    /// <summary>
    /// Middleware that implements a circuit breaker pattern to handle and manage request failures.
    /// </summary>
    public class CircuitBreakerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CircuitBreakerMiddleware> _logger;


        /// <summary>
        /// Represents the current state of the circuit breaker.
        /// </summary>
        private static CircuitBreakerState _state = CircuitBreakerState.Closed;

        /// <summary>
        /// Stores the last time the circuit breaker state was changed.
        /// </summary>
        private static DateTime _lastStateChangeTime;

        /// <summary>
        /// Threshold for the number of failures before opening the circuit.
        /// </summary>
        private readonly int _failureThreshold;

        /// <summary>
        /// Duration for which the circuit remains open before transitioning to half-open.
        /// </summary>
        private readonly TimeSpan _circuitOpenDuration;

        /// <summary>
        /// Duration for which the circuit remains half-open before transitioning to closed.
        /// </summary>
        private readonly TimeSpan _halfOpenDuration;

        /// <summary>
        /// Counts the number of failures that have occurred.
        /// </summary>
        private static int _failureCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="CircuitBreakerMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="configuration">The configuration service to fetch the settings</param>
        public CircuitBreakerMiddleware(RequestDelegate next, IConfigurationService configuration, ILogger<CircuitBreakerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
            _failureThreshold = configuration.Config.CircuitBreaker.FailureThreshold;
            _circuitOpenDuration = TimeSpan.FromSeconds(configuration.Config.CircuitBreaker.CircuitOpenDuration);
            _halfOpenDuration = TimeSpan.FromSeconds(configuration.Config.CircuitBreaker.HalfOpenDuration);

        }

        /// <summary>
        /// Processes an HTTP request and applies the circuit breaker logic.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            switch (_state)
            {
                case CircuitBreakerState.Open:
                    // Check if the circuit open duration has elapsed
                    if (DateTime.UtcNow - _lastStateChangeTime > _circuitOpenDuration)
                    {
                        // Transition to the HalfOpen state
                        _state = CircuitBreakerState.HalfOpen;
                        _logger.LogInformation("The circuit is half open.");
                        _lastStateChangeTime = DateTime.UtcNow;
                    }
                    else
                    {
                        // Respond with 503 Service Unavailable
                        _logger.LogError("503 Service Unavailable, the circuit is open.");
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        await context.Response.WriteAsync("Please try again later.");
                        return;
                    }
                    break;

                case CircuitBreakerState.HalfOpen:
                    // Check if the half-open duration has elapsed
                    if (DateTime.UtcNow - _lastStateChangeTime > _halfOpenDuration)
                    {
                        // Transition to the Closed state
                        _state = CircuitBreakerState.Closed;
                        _logger.LogInformation("The circuit is close.");

                        _lastStateChangeTime = DateTime.UtcNow;
                        _failureCount = 0;
                    }
                    break;
            }

            try
            {
                // Process the request with the next middleware in the pipeline
                await _next(context);

                // If in HalfOpen state, transition to Closed state upon successful request
                if (_state == CircuitBreakerState.HalfOpen)
                {
                    _state = CircuitBreakerState.Closed;
                    _logger.LogInformation("The circuit is close.");
                    _failureCount = 0;
                    _lastStateChangeTime = DateTime.UtcNow;
                }
            }
            catch (Exception)
            {
                // Increment the failure count
                _failureCount++;
                if (_failureCount >= _failureThreshold)
                {
                    // Transition to the Open state if failure threshold is reached
                    _state = CircuitBreakerState.Open;
                    _logger.LogInformation("The circuit is open.");
                    _lastStateChangeTime = DateTime.UtcNow;

                }
                else if (_state == CircuitBreakerState.HalfOpen)
                {
                    // Transition to the Open state if in HalfOpen state and an exception occurs
                    _state = CircuitBreakerState.Open;
                    _logger.LogInformation("The circuit is open.");
                    _lastStateChangeTime = DateTime.UtcNow;
                }
                throw;
            }
        }

        /// <summary>
        /// Enum representing the states of the circuit breaker.
        /// </summary>
        private enum CircuitBreakerState
        {
            Closed = 1,
            Open = 2,
            HalfOpen = 3
        }
    }
}
