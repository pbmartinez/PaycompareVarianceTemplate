using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace PaycompareVariance.UI
{
    public class ClassNameFieldValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult(false, "Field is required.");
            }

            string className = value.ToString();

            // Check if the class name starts with an uppercase letter
            if (!char.IsUpper(className[0]))
            {
                return new ValidationResult(false, "Class name must start with an uppercase letter.");
            }

            // Check if the class name contains only letters, digits, and underscores
            if (!Regex.IsMatch(className, @"^[A-Za-z0-9_]+$"))
            {
                return new ValidationResult(false, "Class name can only contain letters, digits, and underscores.");
            }

            return ValidationResult.ValidResult;
        }
    }
}
