using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CurrencyConverter.Api.ModelBinders
{
    public class CustomDateTimeModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            ArgumentNullException.ThrowIfNull(bindingContext);

            var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return Task.CompletedTask;
            }

            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            if (string.IsNullOrEmpty(value))
            {
                return Task.CompletedTask;
            }

            if (!DateTime.TryParse(value, out DateTime dateTime))
            {
                // Custom behavior: Set a custom error message or handle the error.
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Invalid date format. Please use a valid date like 'yyyy-MM-dd'.");
                return Task.CompletedTask;
            }

            bindingContext.Result = ModelBindingResult.Success(dateTime);
            return Task.CompletedTask;
        }
    }

    public class CustomDateTimeModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            // Apply this binder only to DateTime types
            if (context.Metadata.ModelType == typeof(DateTime))
            {
                return new CustomDateTimeModelBinder();
            }

            return null;
        }
    }
}
