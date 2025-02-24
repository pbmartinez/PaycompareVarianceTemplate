using System.Globalization;
using System.Windows.Controls;

namespace PaycompareVariance.UI
{
    public class RequiredFieldValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Field is required.");
            }
            return ValidationResult.ValidResult;
        }
    }
}
