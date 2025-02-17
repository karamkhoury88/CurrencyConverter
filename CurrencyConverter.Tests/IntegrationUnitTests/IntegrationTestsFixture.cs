using Aspire.Hosting;
using System.Text;
using System.Text.Json;

namespace CurrencyConverter.Tests.IntegrationUnitTests
{
    /// <summary>
    /// Test fixture for integration tests, providing setup and teardown functionality for the test environment.
    /// Implements <see cref="IAsyncLifetime"/> to manage asynchronous initialization and disposal.
    /// </summary>
    public class IntegrationTestsFixture : IAsyncLifetime
    {
        /// <summary>
        /// Gets the <see cref="HttpClient"/> instance configured to interact with the CurrencyConverter API.
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        private DistributedApplication _app; // Represents the distributed application under test

        /// <summary>
        /// Performs cleanup tasks when the test fixture is disposed.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is intentionally left empty for now, but it can be used to dispose of resources like <see cref="HttpClient"/> or <see cref="_app"/> if needed.
        /// </remarks>
        public Task DisposeAsync()
        {
            // Perform cleanup if necessary
            // Example:
            // HttpClient.Dispose();
            // _app.Dispose();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes the test fixture by starting the distributed application and configuring the <see cref="HttpClient"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method sets up the test environment by:
        /// 1. Creating a distributed application host for the CurrencyConverter application.
        /// 2. Configuring the <see cref="HttpClient"/> to interact with the API.
        /// 3. Starting the application and waiting for the API resource to be in a running state.
        /// </remarks>
        public async Task InitializeAsync()
        {
            // Create a distributed application host for the CurrencyConverter application
            IDistributedApplicationTestingBuilder appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.CurrencyConverter_AppHost>();

            // Configure HTTP client defaults (e.g., resilience handlers)
            appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
            {
                // Example: Add a standard resilience handler (currently disabled for smoother test execution)
                // clientBuilder.AddStandardResilienceHandler();
            });

            // Build and start the distributed application
            _app = await appHost.BuildAsync();
            await _app.StartAsync();

            // Wait for the "currencyconverter-api" resource to be in a running state
            ResourceNotificationService resourceNotificationService = _app.Services.GetRequiredService<ResourceNotificationService>();
            await resourceNotificationService.WaitForResourceAsync("currencyconverter-api", KnownResourceStates.Running)
                                            .WaitAsync(TimeSpan.FromSeconds(10));

            // Create an HttpClient instance to interact with the CurrencyConverter API
            HttpClient = _app.CreateHttpClient("currencyconverter-api");
        }
    }
}