using Application.DTOs.Questionnaire;
using FeedBackApp.Backend.Infrastructure.Configuration;
using FluentValidation;

namespace Application.Validation.CreateValidation
{
    public class StudentSetValidator : AbstractValidator<StudentSetDTO>
    {
        public StudentSetValidator()
        {
            RuleFor(dto => dto.SetId).NotEmpty().WithMessage("StudentSets: Studentset needs an ID");
            RuleFor(dto => dto.StudentEmails)
            .NotEmpty()
            .WithMessage("StudentSets: Student email list can not be empty")
            .When(dto => dto.SetId != AuthorizationOptions.UniversalStudentSetId);
            RuleForEach(dto => dto.StudentEmails).EmailAddress().WithMessage("StudentSets: Invalid email adress format: {PropertyValue}");
        }
    }
}
