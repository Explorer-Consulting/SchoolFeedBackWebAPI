using Application.DTOs.Questionnaire;
using FluentValidation;

namespace Application.Validation
{
    public class QuestionnaireValidator : AbstractValidator<QuestionnaireDto>
    {
        public QuestionnaireValidator()
        {
            RuleFor(q => q.SurveyId)
                .NotEmpty().WithMessage("Survey ID cannot be empty");

            RuleFor(q => q.TeacherEmail)
                .NotEmpty().WithMessage("Teacher email cannot be empty")
                .EmailAddress().WithMessage("Invalid teacher email format: {PropertyValue}");

            RuleFor(q => q.StudentEmail)
                .NotEmpty().WithMessage("Student email cannot be empty")
                .EmailAddress().WithMessage("Invalid student email format: {PropertyValue}");

            RuleFor(q => q.SubjectName)
                .NotEmpty().WithMessage("Subject name cannot be empty")
                .MaximumLength(200).WithMessage("Subject name cannot exceed 200 characters. Found: {PropertyValue}");

            RuleFor(q => q.QuestionnaireResults)
                .NotEmpty().WithMessage("Questionnaire must contain at least one answer");

            RuleForEach(q => q.QuestionnaireResults)
                .SetValidator(new QuestionAnswerValidator());
        }
    }
}
