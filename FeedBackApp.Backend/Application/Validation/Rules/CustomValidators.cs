using FluentValidation;
using FluentValidation.Validators;

namespace Application.Validation.Rules
{
    /// <summary>
    /// Custom validators for specific validation scenarios.
    /// These can be used with Must() or as standalone validators.
    /// </summary>
    public static class CustomValidators
    {
        /// <summary>
        /// Validates that a date range is valid (end date is after start date with minimum gap).
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="minimumDays">Minimum number of days between start and end (default: 1)</param>
        /// <returns>True if the date range is valid</returns>
        public static bool IsValidDateRange(DateTime startDate, DateTime endDate, int minimumDays = 1)
        {
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
                return true; // Let other rules handle empty dates

            return endDate >= startDate.AddDays(minimumDays);
        }

        /// <summary>
        /// Validates that a string is a valid GUID format.
        /// </summary>
        /// <param name="value">The string to validate</param>
        /// <returns>True if the string is a valid GUID</returns>
        public static bool IsValidGuid(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Guid.TryParse(value, out _);
        }

        /// <summary>
        /// Validates that all strings in a collection are valid email addresses.
        /// </summary>
        /// <param name="emails">The collection of email strings</param>
        /// <returns>True if all emails are valid</returns>
        public static bool AreAllValidEmails(IEnumerable<string>? emails)
        {
            if (emails == null)
                return false;

            // Simple email validation regex
            var emailRegex = new System.Text.RegularExpressions.Regex(
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            return emails.All(email => 
                !string.IsNullOrWhiteSpace(email) && 
                emailRegex.IsMatch(email));
        }

        /// <summary>
        /// Validates that a string contains no empty or whitespace values.
        /// </summary>
        /// <param name="values">The collection of strings</param>
        /// <returns>True if all values are non-empty</returns>
        public static bool ContainsNoEmptyValues(IEnumerable<string>? values)
        {
            if (values == null)
                return false;

            return values.All(v => !string.IsNullOrWhiteSpace(v));
        }

        /// <summary>
        /// Validates that a collection has at least one non-empty value.
        /// </summary>
        /// <param name="values">The collection of strings</param>
        /// <returns>True if at least one value is non-empty</returns>
        public static bool HasAtLeastOneNonEmptyValue(IEnumerable<string>? values)
        {
            if (values == null)
                return false;

            return values.Any(v => !string.IsNullOrWhiteSpace(v));
        }
    }

    /// <summary>
    /// Custom property validator for GUID format validation.
    /// </summary>
    public class GuidFormatValidator : PropertyValidator<string?, string>
    {
        public override string Name => "GuidFormatValidator";

        protected override string GetDefaultMessageTemplate(string errorCode)
        {
            return "{PropertyName} must be a valid GUID format";
        }

        public override bool IsValid(ValidationContext<string?> context, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Guid.TryParse(value, out _);
        }
    }

    /// <summary>
    /// Custom property validator for date range validation.
    /// </summary>
    public class DateRangeValidator : PropertyValidator<(DateTime StartDate, DateTime EndDate), DateTime>
    {
        private readonly int _minimumDays;

        public DateRangeValidator(int minimumDays = 1)
        {
            _minimumDays = minimumDays;
        }

        public override string Name => "DateRangeValidator";

        protected override string GetDefaultMessageTemplate(string errorCode)
        {
            return $"End date must be at least {_minimumDays} day(s) after start date";
        }

        public override bool IsValid(ValidationContext<(DateTime StartDate, DateTime EndDate)> context, DateTime value)
        {
            var (startDate, endDate) = context.InstanceToValidate;
            
            if (startDate == DateTime.MinValue || endDate == DateTime.MinValue)
                return true; // Let other rules handle empty dates

            return endDate >= startDate.AddDays(_minimumDays);
        }
    }
}

