using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Services.AppServices.Configuration.Dtos
{
    public record BaseConfigurationDto
    {
        // Validate the object against all validation attributes
        public void Validate()
        {
            var validationResults = new List<ValidationResult>();
            var context = new ValidationContext(this, null, null);
            bool isValid = Validator.TryValidateObject(this, context, validationResults, true);
            if (!isValid)
            {
                var errors = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
                throw new InvalidOperationException($"Configuration are invalid, errors: {errors}");
            }
        }
    }
}
