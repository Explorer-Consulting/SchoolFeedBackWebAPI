using Application.DTOs.Questionnaire;
using Application.Validation.Base;
using FluentValidation;

namespace Application.Validation.CreateValidation
{
    public class StudentSetValidator : BaseValidator<StudentSetDTO>
    {
        public StudentSetValidator()
        {
            RuleFor(dto => dto.SetId).NotEmpty().WithMessage("StudentSets: Studentset needs an ID");
            RuleFor(dto => dto.StudentEmails).NotEmpty().WithMessage("StudentSets: Student email list can not be empty");
            RuleForEach(dto => dto.StudentEmails).EmailAddress().WithMessage("StudentSets: Invalid email adress format: {PropertyValue}");
        }
    }
}
