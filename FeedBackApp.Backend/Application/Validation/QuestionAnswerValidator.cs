using Application.DTOs.Questionnaire;
using Application.Validation.Base;
using FluentValidation;

namespace Application.Validation
{
    public class QuestionAnswerValidator : BaseValidator<PostAnswerDto>
    {
        public QuestionAnswerValidator()
        {
        }
    }
}
