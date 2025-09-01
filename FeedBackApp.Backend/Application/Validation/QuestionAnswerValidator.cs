using Application.DTOs.Questionnaire;
using FluentValidation;

namespace Application.Validation
{
    public class QuestionAnswerValidator : AbstractValidator<PostAnswerDto>
    {
        public QuestionAnswerValidator()
        {
        }
    }
}
