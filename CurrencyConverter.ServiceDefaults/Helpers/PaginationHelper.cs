namespace CurrencyConverter.ServiceDefaults.Helpers
{
    public static class PaginationHelper
    {
        /// <summary>
        /// Splits a dictionary into smaller chunks (pages) of key-value pairs.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in the dictionary. Must be sortable.</typeparam>
        /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
        /// <param name="source">The dictionary you want to paginate.</param>
        /// <param name="pageNumber">The page you want to get (starting from 1).</param>
        /// <param name="pageSize">How many items you want on each page.</param>
        /// <returns>A new dictionary with only the items for the requested page.</returns>
        public static Dictionary<TKey, TValue> PaginateDictionary<TKey, TValue>(Dictionary<TKey, TValue> source, int pageNumber, int pageSize) where TKey : IComparable<TKey>
        {
            // Step 1: Sort the keys in ascending order
            // This makes sure the pagination is consistent every time you call the method
            var orderedKeys = source.Keys.OrderBy(k => k).ToList();

            // Step 2: Figure out which keys belong to the requested page
            // Skip the keys from previous pages and take only the keys for the current page
            var paginatedKeys = orderedKeys
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Step 3: Build a new dictionary with the keys and values for the current page
            // This ensures the returned dictionary only has the items for the requested page
            return paginatedKeys.ToDictionary(key => key, key => source[key]);
        }
    }
}
