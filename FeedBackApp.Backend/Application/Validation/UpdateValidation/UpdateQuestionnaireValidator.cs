using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;
using FluentValidation;

namespace Application.Validation.UpdateValidation
{
    public class UpdateQuestionnaireValidator : AbstractValidator<UpdateQuestionnaireDTO>
    {
        public UpdateQuestionnaireValidator(IList<QuestionTemplate> templates)
        {
            RuleForEach(dto => dto.QuestionnaireResult)
                .SetValidator(new QuestionUpdateValidator(templates));
        }
    }
}
