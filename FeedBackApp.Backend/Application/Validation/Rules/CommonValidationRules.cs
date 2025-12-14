using FluentValidation;

namespace Application.Validation.Rules
{
    /// <summary>
    /// Reusable validation rule sets for common patterns.
    /// These rules can be applied to any validator using extension methods.
    /// </summary>
    public static class CommonValidationRules
    {
        /// <summary>
        /// Applies standard email validation rules (NotEmpty + EmailAddress).
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The rule builder for chaining</returns>
        public static IRuleBuilderOptions<T, string> ValidEmail<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            string? propertyName = null)
        {
            var name = propertyName ?? "Email";
            return ruleBuilder
                .NotEmpty()
                .WithMessage($"{name} cannot be empty")
                .EmailAddress()
                .WithMessage($"Invalid {name.ToLower()} format: {{PropertyValue}}");
        }

        /// <summary>
        /// Applies standard email validation rules with custom error message prefix.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder</param>
        /// <param name="errorPrefix">Prefix for error messages (e.g., "Teacher list:")</param>
        /// <returns>The rule builder for chaining</returns>
        public static IRuleBuilderOptions<T, string> ValidEmailWithPrefix<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            string errorPrefix)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage($"{errorPrefix} Email cannot be empty")
                .EmailAddress()
                .WithMessage($"{errorPrefix} Invalid email format: {{PropertyValue}}");
        }

        /// <summary>
        /// Applies standard string length validation rules.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder</param>
        /// <param name="minLength">Minimum length (optional)</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The rule builder for chaining</returns>
        public static IRuleBuilderOptions<T, string> ValidStringLength<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int maxLength,
            int? minLength = null,
            string? propertyName = null)
        {
            var name = propertyName ?? "Property";
            var builder = ruleBuilder
                .NotEmpty()
                .WithMessage($"{name} cannot be empty");

            if (minLength.HasValue)
            {
                builder = builder
                    .MinimumLength(minLength.Value)
                    .WithMessage($"{name} must be at least {minLength.Value} characters long");
            }

            return builder
                .MaximumLength(maxLength)
                .WithMessage($"{name} cannot exceed {maxLength} characters. Found: {{PropertyValue}}");
        }

        /// <summary>
        /// Applies standard string length validation with custom error message prefix.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="errorPrefix">Prefix for error messages</param>
        /// <param name="minLength">Minimum length (optional)</param>
        /// <returns>The rule builder for chaining</returns>
        public static IRuleBuilderOptions<T, string> ValidStringLengthWithPrefix<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            int maxLength,
            string errorPrefix,
            int? minLength = null)
        {
            var builder = ruleBuilder
                .NotEmpty()
                .WithMessage($"{errorPrefix} cannot be empty");

            if (minLength.HasValue)
            {
                builder = builder
                    .MinimumLength(minLength.Value)
                    .WithMessage($"{errorPrefix} must be at least {minLength.Value} characters long");
            }

            return builder
                .MaximumLength(maxLength)
                .WithMessage($"{errorPrefix} cannot exceed {maxLength} characters. Found: {{PropertyValue}}");
        }

        /// <summary>
        /// Validates that a string is a valid GUID format.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The rule builder for chaining</returns>
        public static IRuleBuilderOptions<T, string> ValidGuid<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            string? propertyName = null)
        {
            var name = propertyName ?? "ID";
            return ruleBuilder
                .NotEmpty()
                .WithMessage($"{name} cannot be empty")
                .Must(BeValidGuidFormat)
                .WithMessage($"{name} must be a valid GUID format");
        }

        /// <summary>
        /// Validates that a date is not empty and is a valid date.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="ruleBuilder">The rule builder</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The rule builder for chaining</returns>
        public static IRuleBuilderOptions<T, DateTime> ValidDate<T>(
            this IRuleBuilder<T, DateTime> ruleBuilder,
            string? propertyName = null)
        {
            var name = propertyName ?? "Date";
            return ruleBuilder
                .NotEmpty()
                .WithMessage($"{name} cannot be empty")
                .GreaterThan(DateTime.MinValue)
                .WithMessage($"{name} must be a valid date");
        }


        /// <summary>
        /// Validates that a collection is not empty.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <typeparam name="TElement">The element type of the collection</typeparam>
        /// <param name="ruleBuilder">The rule builder</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The rule builder for chaining</returns>
        public static IRuleBuilderOptions<T, IEnumerable<TElement>> NotEmptyCollection<T, TElement>(
            this IRuleBuilder<T, IEnumerable<TElement>> ruleBuilder,
            string? propertyName = null)
        {
            var name = propertyName ?? "Collection";
            return ruleBuilder
                .NotEmpty()
                .WithMessage($"{name} cannot be empty");
        }

        /// <summary>
        /// Helper method to check if a string is a valid GUID format.
        /// </summary>
        private static bool BeValidGuidFormat(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Guid.TryParse(value, out _);
        }
    }
}

