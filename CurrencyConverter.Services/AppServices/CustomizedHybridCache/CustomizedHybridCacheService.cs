using Microsoft.Extensions.Caching.Hybrid;

namespace CurrencyConverter.Services.AppServices.CustomizedHybridCache
{
    /// <summary>
    /// Internal implementation of the customized hybrid cache service.
    /// Provides methods for caching and retrieving data using a hybrid cache.
    /// </summary>
    internal class CustomizedHybridCacheService : ICustomizedHybridCacheService
    {
        private readonly HybridCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomizedHybridCacheService"/> class with the specified hybrid cache.
        /// </summary>
        /// <param name="cache">The hybrid cache instance.</param>
        public CustomizedHybridCacheService(HybridCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Gets an item from the cache or creates it using the specified factory function.
        /// </summary>
        /// <typeparam name="T">The type of the item to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="factory">The factory function to create the item if it's not found in the cache.</param>
        /// <param name="options">Optional cache entry options for customizing the cache behavior.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous cache get or create operation. The task result contains the cached or created item.</returns>
        public ValueTask<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions? options = null, CancellationToken cancellationToken = default)
            => _cache.GetOrCreateAsync(
                key: key,
                factory: factory,
                options: options,
                cancellationToken: cancellationToken);
    }
}
