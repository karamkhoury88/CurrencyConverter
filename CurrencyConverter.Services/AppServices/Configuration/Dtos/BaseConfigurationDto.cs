using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    /// <summary>
    /// Represents a base class for configuration data transfer objects (DTOs).
    /// Provides a method to validate the object using data annotations.
    /// </summary>
    public record BaseConfigurationDto
    {
        /// <summary>
        /// Validates the object using data annotations and throws an exception if validation fails.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the object fails validation.</exception>
        public void Validate()
        {
            // Create a list to store validation results.
            var validationResults = new List<ValidationResult>();

            // Create a validation context for the current object.
            var context = new ValidationContext(this, null, null);

            // Attempt to validate the object using data annotations.
            bool isValid = Validator.TryValidateObject(this, context, validationResults, validateAllProperties: true);

            // If validation fails, throw an exception with the error messages.
            if (!isValid)
            {
                var errors = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
                throw new InvalidOperationException($"Configuration is invalid. Errors: {errors}");
            }
        }
    }
}