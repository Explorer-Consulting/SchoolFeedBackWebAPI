using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;
using FluentValidation;
using FeedBackApp.Core.Model.Enum;

namespace Application.Validation.UpdateValidation
{
    public class UpdateQuestionnaireValidator : AbstractValidator<UpdateQuestionnaireDTO>
    {
        public UpdateQuestionnaireValidator(IList<QuestionTemplate> templates)
        {
            RuleForEach(dto => dto.QuestionnaireResult)
                .SetValidator(new QuestionUpdateValidator(templates));

            RuleFor(dto => dto).Custom((dto, context) => {
                foreach (var result in dto.QuestionnaireResult)
                {
                    var template = templates.FirstOrDefault(t => t.Id == result.QuestionId);
                    if (template == null)
                        continue;


                    if (template.Type == QuestionType.FilteredDropDown && !string.IsNullOrWhiteSpace(result.Answer))
                    {
                        var parentAnswer = dto.QuestionnaireResult
                            .FirstOrDefault(r => r.QuestionId == template.ParentQuestionId)?.Answer;

                        var chosen = template.DropDownItems
                            .FirstOrDefault(it => string.Equals(it.Id, result.Answer, StringComparison.Ordinal));

                        if (chosen != null && !string.Equals(chosen.ParentId, parentAnswer, StringComparison.Ordinal))
                        {
                            context.AddFailure($"Answer for '{template.Question}-{template.Id}' does not belong to the selected option of '{template.ParentQuestionId}'.");
                        }
                    }
                }
            });
        }
    }
}
