using Application.DTOs.Questionnaire;
using FluentValidation;

namespace Application.Validation
{
    public class MetaTeacherValidator : AbstractValidator<MetaTeacherDto>
    {
        public MetaTeacherValidator() 
        {
            RuleFor(dto => dto.Email).NotEmpty().WithMessage("Teacher list: Teacher email can not be empty")
                .EmailAddress().WithMessage("Teacher list: Incorrect teacher email adress format: {PropertyValue}");
            RuleFor(dto => dto.Name).NotEmpty().WithMessage("Teacher list: Teacher name can not be empty")
                .MaximumLength(100).WithMessage("Teacher list: Teacher name can not exceed 100 characters. Found: {PropertyValue}");
        }
    }
}
