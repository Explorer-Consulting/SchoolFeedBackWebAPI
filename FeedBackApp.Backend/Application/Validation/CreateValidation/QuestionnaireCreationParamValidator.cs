using Application.DTOs.Questionnaire;
using FluentValidation;

namespace Application.Validation.CreateValidation
{
    public class QuestionnaireCreationParamValidator : AbstractValidator<QuestionnaireCreationParamDTO>
    {
        public QuestionnaireCreationParamValidator()
        {
            RuleFor(dto=> dto.TeacherEmail)
                .NotEmpty().WithMessage("CreationParams: Teacher email cannot be empty")
                .EmailAddress().WithMessage("CreationParams: Invalid teacher email format: {PropertyValue}");

            RuleFor(dto=> dto.SubjectName)
                .NotEmpty().WithMessage("CreationParams: Subject name cannot be empty")
                .MaximumLength(200).WithMessage("CreationParams: Subject name cannot exceed 200 characters. Found: {PropertyValue}");

            RuleFor(dto=> dto.StudentSetIds)
                .NotEmpty().WithMessage("CreationParams: At least one student set ID must be provided");

            RuleForEach(dto=> dto.StudentSetIds)
                .NotEmpty().WithMessage("CreationParams: Student set ID cannot be empty");

        }

    }
}
