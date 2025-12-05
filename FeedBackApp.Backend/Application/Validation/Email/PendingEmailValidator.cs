using Application.DTOs.Email;
using Application.Validation.Base;
using FluentValidation;

namespace Application.Validation.Email
{
    public class PendingEmailValidator : BaseValidator<PendingEmailDTO>
    {
        public PendingEmailValidator()
        {
            // NotEmpty()
            RuleFor(dto => dto.SurveyId)
                .NotEmpty()
                .WithMessage("Survey ID cannot be empty");

            // Length()
            RuleFor(dto => dto.SurveyName)
                .NotEmpty()
                .WithMessage("Survey name cannot be empty")
                .MinimumLength(3)
                .WithMessage("Survey name must be at least 3 characters long")
                .MaximumLength(200)
                .WithMessage("Survey name cannot exceed 200 characters. Found: {PropertyValue}");

            // EmailAddress()
            RuleFor(dto => dto.Email)
                .NotEmpty()
                .WithMessage("Email address cannot be empty")
                .EmailAddress()
                .WithMessage("Invalid email format: {PropertyValue}");

            // Date validation
            RuleFor(dto => dto.StartDate)
                .NotEmpty()
                .WithMessage("Start date cannot be empty")
                .GreaterThan(DateTime.MinValue)
                .WithMessage("Start date must be a valid date");

            RuleFor(dto => dto.EndDate)
                .NotEmpty()
                .WithMessage("End date cannot be empty")
                .GreaterThan(DateTime.MinValue)
                .WithMessage("End date must be a valid date");

            // crossfield validation
            RuleFor(dto => dto.EndDate)
                .GreaterThan(dto => dto.StartDate)
                .WithMessage("End date must be after start date. Start: {ComparisonValue}, End: {PropertyValue}")
                .When(dto => dto.StartDate != DateTime.MinValue && dto.EndDate != DateTime.MinValue);

            // Must() - custom validation logic
            RuleFor(dto => dto.SurveyId)
                .Must(BeValidSurveyIdFormat)
                .WithMessage("Survey ID must be a valid GUID format")
                .When(dto => !string.IsNullOrWhiteSpace(dto.SurveyId));

            // custom validation
            RuleFor(dto => dto)
                .Must(HaveValidDateRange)
                .WithMessage("Date range is invalid: end date must be at least 1 day after start date")
                .When(dto => dto.StartDate != DateTime.MinValue && dto.EndDate != DateTime.MinValue);
        }

        private static bool BeValidSurveyIdFormat(string? surveyId)
        {
            if (string.IsNullOrWhiteSpace(surveyId))
                return false;

            return Guid.TryParse(surveyId, out _);
        }

        private static bool HaveValidDateRange(PendingEmailDTO dto)
        {
            if (dto.StartDate == DateTime.MinValue || dto.EndDate == DateTime.MinValue)
                return true; // Let other rules handle empty dates

            // ensure end date is at least 1 day after start date
            return dto.EndDate >= dto.StartDate.AddDays(1);
        }
    }
}


