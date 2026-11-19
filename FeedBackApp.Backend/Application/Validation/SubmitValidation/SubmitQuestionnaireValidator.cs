using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;
using FluentValidation;
using FeedBackApp.Core.Model.Enum;

namespace Application.Validation.SubmitValidation
{
    public class SubmitQuestionnaireValidator : AbstractValidator<SubmitQuestionnaireDTO>
    {
        public SubmitQuestionnaireValidator(IList<QuestionTemplate> templates)
        {
            RuleForEach(dto => dto.QuestionnaireResult)
                .SetValidator(new QuestionSubmitValidator(templates));

            RuleFor(dto => dto)
                .Custom((dto, context) =>
                {
                    var answers = dto.QuestionnaireResult
                        .GroupBy(r => r.QuestionId)
                        .ToDictionary(g => g.Key, g => g.First().Answer);

                    var templatesById = templates.ToDictionary(t => t.Id);
                    var memo = new Dictionary<string, bool>();

                    bool IsActive(string id)
                    {
                        if (memo.TryGetValue(id, out var cached)) return cached;
                        if (!templatesById.TryGetValue(id, out var t)) return memo[id] = false;

                        bool active = true;

                        if (t.Dependency != null)
                        {
                            active = IsActive(t.Dependency.Id)
                                && int.TryParse(answers.GetValueOrDefault(t.Dependency.Id), out var parentValue)
                                && t.Dependency.AnswerConditions.Contains(parentValue);
                        }

                        if (active && !string.IsNullOrEmpty(t.ParentQuestionId))
                            active = IsActive(t.ParentQuestionId);

                        return memo[id] = active;
                    }

                    foreach (var result in dto.QuestionnaireResult)
                    {
                        if (!templatesById.TryGetValue(result.QuestionId, out var template))
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

                        if (IsActive(template.Id) && string.IsNullOrWhiteSpace(result.Answer))
                        {
                            context.AddFailure($"Answer for '{template.Question}-{template.Id}' cannot be empty.");
                        }
                    }
                });
        }
    }
}
