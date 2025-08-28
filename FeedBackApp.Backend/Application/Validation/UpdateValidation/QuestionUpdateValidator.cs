using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FluentValidation;

namespace Application.Validation.UpdateValidation
{
    public class QuestionUpdateValidator : AbstractValidator<QuestionResultDTO>
    {
        public QuestionUpdateValidator(IList<QuestionTemplate> templates)
        {
            RuleFor(dto => dto.QuestionId)
                .NotEmpty().WithMessage("QuestionId cannot be null/empty")
                .Must(id => templates.Any(t => t.Id == id))
                .WithMessage(dto => $"Question with id {dto.QuestionId} does not exist.");

            RuleFor(dto => dto.Answer)
                .Custom((answer, context) =>
                {
                    var dtoInstance = (QuestionResultDTO)context.InstanceToValidate;
                    var template = templates.FirstOrDefault(t => t.Id == dtoInstance.QuestionId);

                    if (template == null)
                        return;

                    if (string.IsNullOrWhiteSpace(answer))
                        return;

                    switch (template.Type)
                    {
                        case QuestionType.MultinomialSingleChoice:
                            if (!int.TryParse(answer, out int singleChoice) || singleChoice < 1 || singleChoice > template.AnswerOptions.Count)
                                context.AddFailure("Answer", $"Answer must be a number between 0 and {template.AnswerOptions.Count} for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.MultipleChoice:
                            var parts = answer.Split('-', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var part in parts)
                            {
                                if (!int.TryParse(part, out int choice) || choice < 1 || choice > template.AnswerOptions.Count)
                                {
                                    context.AddFailure("Answer", $"Answer index {part} is invalid for '{template.Question}-{template.Id}'.");
                                }
                            }
                            break;

                        case QuestionType.LikertScaleOneToFive:
                            if (!int.TryParse(answer, out int scale) || scale < 1 || scale > 5)
                                context.AddFailure("Answer", $"Answer must be a number between 0 and 5 for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.MultiNomialSingleChoiceOther:
                            if (int.TryParse(answer, out int singleChoice2))
                            {
                                if (singleChoice2 < 1 || singleChoice2 > template.AnswerOptions.Count)
                                {
                                    context.AddFailure("Answer", $"Answer must be a number between 1 and {template.AnswerOptions.Count} for '{template.Question}-{template.Id}'.");
                                }
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(answer))
                                {
                                    context.AddFailure("Answer", $"Answer cannot be empty for '{template.Question}-{template.Id}'.");
                                }
                            }
                            break;

                    }
                });
        }
    }
}
