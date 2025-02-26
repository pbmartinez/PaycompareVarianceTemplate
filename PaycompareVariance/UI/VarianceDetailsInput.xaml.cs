﻿using System.Windows;
using System.Windows.Controls;

namespace PaycompareVariance.UI
{
    /// <summary>
    /// Interaction logic for VarianceDetailsInput.xaml
    /// </summary>
    public partial class VarianceDetailsInput : Window
    {
        public bool IsValid { get; set; } = false;

        public VarianceDetailsInput()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInputs())
            {
                MessageBox.Show("Please correct the validation errors.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            IsValid = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IsValid = false;
            Close();
        }

        private bool ValidateInputs()
        {
            var requiredValidationRule = new RequiredFieldValidationRule();
            var classNameValidationRule = new ClassNameFieldValidationRule();
            bool isValid = true;

            isValid &= ValidateTextBox(JiraTextBox, requiredValidationRule);
            isValid &= ValidateTextBox(NameTextBox, requiredValidationRule);
            isValid &= ValidateTextBox(NameTextBox, classNameValidationRule);
            isValid &= ValidateTextBox(DescriptionTextBox, requiredValidationRule);

            return isValid;
        }

        private bool ValidateTextBox(TextBox textBox, ValidationRule validationRule)
        {
            var validationResult = validationRule.Validate(textBox.Text, null);
            if (!validationResult.IsValid)
            {
                Validation.MarkInvalid(textBox.GetBindingExpression(TextBox.TextProperty),
                    new ValidationError(validationRule, textBox.GetBindingExpression(TextBox.TextProperty), validationResult.ErrorContent, null));
                return false;
            }
            else
            {
                Validation.ClearInvalid(textBox.GetBindingExpression(TextBox.TextProperty));
                return true;
            }
        }
    }
}
