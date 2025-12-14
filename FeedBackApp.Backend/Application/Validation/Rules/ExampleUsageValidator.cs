using Application.DTOs.Email;
using Application.Validation.Base;
using Application.Validation.Rules;
using FluentValidation;

namespace Application.Validation.Rules
{
    /// <summary>
    /// Example validator demonstrating how to use reusable validation rules and patterns.
    /// This shows the recommended approach for applying common validation rules.
    /// </summary>
    public class ExampleUsageValidator : BaseValidator<PendingEmailDTO>
    {
        public ExampleUsageValidator()
        {
            // Example 1: Using extension methods for common patterns
            RuleFor(dto => dto.Email)
                .ValidEmail("Email");  // Uses ValidEmail extension

            RuleFor(dto => dto.SurveyName)
                .ValidStringLength(200, minLength: 3, propertyName: "Survey name");  // Uses ValidStringLength extension

            RuleFor(dto => dto.SurveyId)
                .ValidGuid("Survey ID");  // Uses ValidGuid extension

            RuleFor(dto => dto.StartDate)
                .ValidDate("Start date");  // Uses ValidDate extension

            RuleFor(dto => dto.EndDate)
                .ValidDate("End date");

            // Example 2: Using custom validators with Must()
            RuleFor(dto => dto.SurveyId)
                .Must(CustomValidators.IsValidGuid)
                .WithMessage("Survey ID must be a valid GUID format")
                .When(dto => !string.IsNullOrWhiteSpace(dto.SurveyId));

            // Example 3: Cross-field validation using custom validators
            // Note: For date range validation, use CustomValidators.IsValidDateRange with Must()
            RuleFor(dto => dto)
                .Must(dto => CustomValidators.IsValidDateRange(dto.StartDate, dto.EndDate, minimumDays: 1))
                .WithMessage("End date must be at least 1 day after start date")
                .When(dto => dto.StartDate != DateTime.MinValue && dto.EndDate != DateTime.MinValue);

            // Alternative: Use SharedRuleSets for date range validation
            // this.ApplyDateRangeRules(dto => dto.EndDate, dto => dto.StartDate, "End date", minimumDays: 1);

            // Example 4: Using SharedRuleSets (alternative approach)
            // This.ApplyEmailRules(dto => dto.Email, "Email");
            // This.ApplyStringLengthRules(dto => dto.SurveyName, 200, minLength: 3, "Survey name");
            // This.ApplyGuidRules(dto => dto.SurveyId, "Survey ID");
            // This.ApplyDateRules(dto => dto.StartDate, "Start date");
            // This.ApplyDateRules(dto => dto.EndDate, "End date");
            // This.ApplyDateRangeRules(dto => dto.EndDate, dto => dto.StartDate, "End date", minimumDays: 1);
        }
    }
}

