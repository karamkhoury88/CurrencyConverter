using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CurrencyConverter.Api.ModelBinders
{
    /// <summary>
    /// Custom model binder for parsing DateTime values from request data.
    /// </summary>
    public class CustomDateTimeModelBinder : IModelBinder
    {
        /// <summary>
        /// Attempts to bind a model by parsing a DateTime value from the request.
        /// </summary>
        /// <param name="bindingContext">The context for model binding.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Ensure the binding context is not null.
            ArgumentNullException.ThrowIfNull(bindingContext);

            // Retrieve the value from the value provider.
            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            // If no value is found, return without setting a model.
            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            // Set the model value in the model state.
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            // Get the first value from the value provider result.
            var value = valueProviderResult.FirstValue;

            // If the value is null or empty, return without setting a model.
            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            // Attempt to parse the value as a DateTime.
            if (!DateTime.TryParse(value, out DateTime dateTime))
            {
                // If parsing fails, add a custom error message to the model state.
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date format. Please use a valid date like 'yyyy-MM-dd'.");
                return Task.CompletedTask;
            }

            // If parsing succeeds, set the result as the parsed DateTime.
            bindingContext.Result = ModelBindingResult.Success(dateTime);
            return Task.CompletedTask;
        }
    }
}