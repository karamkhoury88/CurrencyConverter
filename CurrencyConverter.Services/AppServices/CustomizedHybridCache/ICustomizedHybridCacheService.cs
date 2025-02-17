using Microsoft.Extensions.Caching.Hybrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CurrencyConverter.Services.AppServices.CustomizedHybridCache
{
    /// <summary>
    /// Interface for a customized hybrid cache service.
    /// Provides methods for caching and retrieving data.
    /// </summary>
    public interface ICustomizedHybridCacheService
    {
        /// <summary>
        /// Gets an item from the cache or creates it using the specified factory function.
        /// </summary>
        /// <typeparam name="T">The type of the item to cache.</typeparam>
        /// <param name="key">The cache key.</param>
        /// <param name="factory">The factory function to create the item if it's not found in the cache.</param>
        /// <param name="options">Optional cache entry options for customizing the cache behavior.</param>
        /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
        /// <returns>A task that represents the asynchronous cache get or create operation. The task result contains the cached or created item.</returns>
        ValueTask<T> GetOrCreateAsync<T>(string key, Func<CancellationToken, ValueTask<T>> factory, HybridCacheEntryOptions? options = null, CancellationToken cancellationToken = default);
    }
}
