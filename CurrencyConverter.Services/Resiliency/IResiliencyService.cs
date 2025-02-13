using CurrencyConverter.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;


namespace CurrencyConverter.Services.Resiliency
{
    public interface IResiliencyService
    {
        AsyncPolicyWrap<HttpResponseMessage> ResiliencyPolicy { get; }
    }

    internal class ResiliencyService : IResiliencyService
    {
        public AsyncPolicyWrap<HttpResponseMessage> ResiliencyPolicy { get; }

        public ResiliencyService(IConfigurationService configuration, ILogger<ResiliencyService> logger)
        {
            int maxRetryCount = configuration.Config.PolyResiliencePolicy.MaxRetryCount;
            int durationOfBreak = configuration.Config.PolyResiliencePolicy.DurationOfBreak;

            // Retry policy with exponential backoff
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: maxRetryCount,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        logger.LogInformation("Retry attempt {RetryAttempt} after {ElapsedSeconds} seconds due to: {ErrorDetails}",
                                                retryAttempt,
                                                timespan.TotalSeconds,
                                                outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
                                            );
                    });

            // Circuit breaker policy
            var circuitBreakerPolicy = Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: maxRetryCount + 1, // 1 initial request + 5 retries
                    durationOfBreak: TimeSpan.FromSeconds(durationOfBreak),
                    onBreak: (outcome, breakDelay, context) =>
                    {
                        logger.LogInformation("Circuit broken! Will not attempt requests for {BreakDelaySeconds} seconds due to: {ErrorDetails}",
                                                breakDelay.TotalSeconds,
                                                outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()
                                            );
                    },
                    onReset: (context) =>
                    {
                        logger.LogInformation("Circuit reset! Requests are allowed again.");
                    },
                    onHalfOpen: () =>
                    {
                        logger.LogInformation("Circuit half-open: Testing the next request.");
                    });

            // Combine policies (retry inside circuit breaker)
            ResiliencyPolicy = Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }
    }
}
