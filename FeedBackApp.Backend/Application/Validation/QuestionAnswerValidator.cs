using Application.DTOs.Questionnaire.Post;
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
