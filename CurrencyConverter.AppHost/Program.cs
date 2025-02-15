using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);
// Read the environment name from configuration
var environmentName = builder.Configuration["Environment"] ?? "Development";
builder.Environment.EnvironmentName = environmentName;


var distributedCache = builder.AddRedis("distributedCache")
    .WithLifetime(ContainerLifetime.Persistent);

  var distributedCacheInsights = distributedCache.WithRedisInsight();

builder.AddProject<Projects.CurrencyConverter_Api>("currencyconverter-api")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
    .WithReplicas(3)
    .WithReference(distributedCache)
    .WaitFor(distributedCache)
    .WaitFor(distributedCacheInsights);

await builder.Build().RunAsync();

