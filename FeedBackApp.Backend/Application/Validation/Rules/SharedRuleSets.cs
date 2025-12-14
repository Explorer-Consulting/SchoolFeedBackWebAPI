using FluentValidation;

namespace Application.Validation.Rules
{
    /// <summary>
    /// Shared rule sets that can be applied to multiple validators.
    /// These provide reusable validation patterns for common scenarios.
    /// </summary>
    public static class SharedRuleSets
    {
        /// <summary>
        /// Applies standard email validation rules to a property.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The validator</param>
        /// <param name="propertySelector">Selector for the email property</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The validator for chaining</returns>
        public static AbstractValidator<T> ApplyEmailRules<T>(
            this AbstractValidator<T> validator,
            System.Linq.Expressions.Expression<Func<T, string>> propertySelector,
            string? propertyName = null)
        {
            validator.RuleFor(propertySelector)
                .ValidEmail(propertyName);
            return validator;
        }

        /// <summary>
        /// Applies standard string length validation rules to a property.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The validator</param>
        /// <param name="propertySelector">Selector for the string property</param>
        /// <param name="maxLength">Maximum length</param>
        /// <param name="minLength">Minimum length (optional)</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The validator for chaining</returns>
        public static AbstractValidator<T> ApplyStringLengthRules<T>(
            this AbstractValidator<T> validator,
            System.Linq.Expressions.Expression<Func<T, string>> propertySelector,
            int maxLength,
            int? minLength = null,
            string? propertyName = null)
        {
            validator.RuleFor(propertySelector)
                .ValidStringLength(maxLength, minLength, propertyName);
            return validator;
        }

        /// <summary>
        /// Applies standard GUID validation rules to a property.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The validator</param>
        /// <param name="propertySelector">Selector for the ID property</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The validator for chaining</returns>
        public static AbstractValidator<T> ApplyGuidRules<T>(
            this AbstractValidator<T> validator,
            System.Linq.Expressions.Expression<Func<T, string>> propertySelector,
            string? propertyName = null)
        {
            validator.RuleFor(propertySelector)
                .ValidGuid(propertyName);
            return validator;
        }

        /// <summary>
        /// Applies standard date validation rules to a property.
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The validator</param>
        /// <param name="propertySelector">Selector for the date property</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <returns>The validator for chaining</returns>
        public static AbstractValidator<T> ApplyDateRules<T>(
            this AbstractValidator<T> validator,
            System.Linq.Expressions.Expression<Func<T, DateTime>> propertySelector,
            string? propertyName = null)
        {
            validator.RuleFor(propertySelector)
                .ValidDate(propertyName);
            return validator;
        }

        /// <summary>
        /// Applies date range validation (end date after start date with minimum gap).
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The validator</param>
        /// <param name="endDateSelector">Selector for the end date property</param>
        /// <param name="startDateSelector">Selector for the start date property</param>
        /// <param name="propertyName">Optional custom property name for error messages</param>
        /// <param name="minimumDays">Minimum number of days between start and end (default: 1)</param>
        /// <returns>The validator for chaining</returns>
        public static AbstractValidator<T> ApplyDateRangeRules<T>(
            this AbstractValidator<T> validator,
            System.Linq.Expressions.Expression<Func<T, DateTime>> endDateSelector,
            System.Linq.Expressions.Expression<Func<T, DateTime>> startDateSelector,
            string? propertyName = null,
            int minimumDays = 1)
        {
            var name = propertyName ?? "End date";
            validator.RuleFor(endDateSelector)
                .GreaterThan(startDateSelector)
                .WithMessage($"{name} must be after start date. Start: {{ComparisonValue}}, End: {{PropertyValue}}")
                .When(dto =>
                {
                    var startDate = startDateSelector.Compile()(dto);
                    var endDate = endDateSelector.Compile()(dto);
                    return startDate != DateTime.MinValue && endDate != DateTime.MinValue;
                });

            // Additional validation for minimum days gap
            validator.RuleFor(dto => dto)
                .Must(dto =>
                {
                    var startDate = startDateSelector.Compile()(dto);
                    var endDate = endDateSelector.Compile()(dto);
                    return CustomValidators.IsValidDateRange(startDate, endDate, minimumDays);
                })
                .WithMessage($"{name} must be at least {minimumDays} day(s) after start date")
                .When(dto =>
                {
                    var startDate = startDateSelector.Compile()(dto);
                    var endDate = endDateSelector.Compile()(dto);
                    return startDate != DateTime.MinValue && endDate != DateTime.MinValue;
                });

            return validator;
        }

        /// <summary>
        /// Applies email list validation (collection of emails, each validated).
        /// </summary>
        /// <typeparam name="T">The type being validated</typeparam>
        /// <param name="validator">The validator</param>
        /// <param name="propertySelector">Selector for the email list property</param>
        /// <param name="errorPrefix">Prefix for error messages</param>
        /// <returns>The validator for chaining</returns>
        public static AbstractValidator<T> ApplyEmailListRules<T>(
            this AbstractValidator<T> validator,
            System.Linq.Expressions.Expression<Func<T, IEnumerable<string>>> propertySelector,
            string? errorPrefix = null)
        {
            var prefix = errorPrefix ?? string.Empty;
            validator.RuleFor(propertySelector)
                .NotEmpty()
                .WithMessage($"{prefix}Email list cannot be empty");
            
            validator.RuleForEach(propertySelector)
                .EmailAddress()
                .WithMessage($"{prefix}Invalid email format: {{PropertyValue}}");
            
            return validator;
        }
    }
}

