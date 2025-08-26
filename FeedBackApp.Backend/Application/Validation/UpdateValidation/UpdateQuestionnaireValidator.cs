using Application.DTOs.Questionnaire;
using FeedBackApp.Core.Model;
using FluentValidation;
using System.Collections.Generic;

namespace Application.Validation.UpdateValidation
{
    public class UpdateQuestionnaireValidator : AbstractValidator<UpdateQuestionnaireDTO>
    {
        public UpdateQuestionnaireValidator(IList<QuestionTemplate> templates)
        {
            RuleForEach(dto => dto.QuestionnaireResult)
                .SetValidator(new QuestionResultValidator(templates));
        }
    }
}
