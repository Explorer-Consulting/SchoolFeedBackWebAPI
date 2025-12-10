using Application.DTOs.Evaluation;
using Application.Validation.Base;
using FeedBackApp.Core.Model;
using FluentValidation;

namespace Application.Validation.UpdateValidation
{
    public class UpdateQuestionnaireValidator : BaseValidator<UpdateQuestionnaireDTO>
    {
        public UpdateQuestionnaireValidator(IList<QuestionTemplate> templates)
        {
            RuleForEach(dto => dto.QuestionnaireResult)
                .SetValidator(new QuestionUpdateValidator(templates));
        }
    }
}
