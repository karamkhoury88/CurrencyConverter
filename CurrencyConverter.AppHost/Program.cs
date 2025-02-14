using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var distributedCache = builder.AddRedis("distributedCache")
    .WithLifetime(ContainerLifetime.Persistent);

  var distributedCacheInsights = distributedCache.WithRedisInsight();

builder.AddProject<Projects.CurrencyConverter_Api>("currencyconverter-api")
    .WithReference(distributedCache)
    .WaitFor(distributedCache)
    .WaitFor(distributedCacheInsights);

await builder.Build().RunAsync();
