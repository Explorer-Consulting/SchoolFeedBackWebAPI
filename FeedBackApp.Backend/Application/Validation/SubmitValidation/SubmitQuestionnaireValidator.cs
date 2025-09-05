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

            RuleFor(dto => dto)
                .Custom((dto, context) =>
                {
                    foreach (var result in dto.QuestionnaireResult)
                    {
                        var template = templates.FirstOrDefault(t => t.Id == result.QuestionId);
                        if (template == null)
                            continue;

                        if (template.Dependency == null)
                        {
                            if (string.IsNullOrWhiteSpace(result.Answer))
                            {
                                context.AddFailure($"Answer for '{template.Question}-{template.Id}' cannot be empty.");
                            }
                            continue;
                        }

                        var parent = dto.QuestionnaireResult
                            .FirstOrDefault(r => r.QuestionId == template.Dependency.Id);

                        if (parent == null)
                            continue;

                        if (int.TryParse(parent.Answer, out int parentValue) &&
                            template.Dependency.AnswerConditions.Contains(parentValue))
                        {
                            if (string.IsNullOrWhiteSpace(result.Answer))
                            {
                                context.AddFailure($"Answer for '{template.Question}-{template.Id}' is required because dependency '{template.Dependency.Id}' was satisfied.");
                            }
                        }
                    }
                });
        }
    }
}
