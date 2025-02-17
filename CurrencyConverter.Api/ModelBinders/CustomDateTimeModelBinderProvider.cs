using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CurrencyConverter.Api.ModelBinders
{
    /// <summary>
    /// Provider for the custom DateTime model binder.
    /// </summary>
    public class CustomDateTimeModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// Gets the appropriate model binder for the specified context.
        /// </summary>
        /// <param name="context">The context for model binding.</param>
        /// <returns>An instance of <see cref="CustomDateTimeModelBinder"/> if the model type is DateTime; otherwise, null.</returns>
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            // Ensure the context is not null.
            ArgumentNullException.ThrowIfNull(context);

            // Apply this binder only to DateTime types.
            if (context.Metadata.ModelType == typeof(DateTime))
            {
                return new CustomDateTimeModelBinder();
            }

            // Return null for other types.
            return null;
        }
    }
}