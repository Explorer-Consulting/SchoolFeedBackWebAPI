
using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;
using FluentValidation;

namespace Application.Validation.SubmitValidation
{
    public class SubmitQuestionnaireValidator : AbstractValidator<SubmitQuestionnaireDTO>
    {
        public SubmitQuestionnaireValidator(IList<QuestionTemplate> templates)
        {
            RuleForEach(dto => dto.QuestionnaireResult)
                .SetValidator(new QuestionSubmitValidator(templates));
        }
    }
}
