using Application.DTOs.Questionnaire;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FluentValidation;

namespace Application.Validation.UpdateValidation
{
    public class QuestionResultValidator : AbstractValidator<QuestionResultDTO>
    {
        public QuestionResultValidator(IList<QuestionTemplate> templates)
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

                    switch (template.Type)
                    {
                        case QuestionType.OpenEnded:
                            if (string.IsNullOrWhiteSpace(answer))
                                context.AddFailure("Answer", $"Answer cannot be empty for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.MultinomialSingleChoice:
                            if (!int.TryParse(answer, out int singleChoice) || singleChoice < 0 || singleChoice >= template.AnswerOptions.Count)
                                context.AddFailure("Answer", $"Answer must be a number between 0 and {template.AnswerOptions.Count - 1} for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.MultipleChoice:
                            var parts = answer.Split('-', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var part in parts)
                            {
                                if (!int.TryParse(part, out int choice) || choice < 0 || choice >= template.AnswerOptions.Count)
                                {
                                    context.AddFailure("Answer", $"Answer index {part} is invalid for '{template.Question}-{template.Id}'.");
                                }
                            }
                            break;

                        case QuestionType.LikertScaleOneToFive:
                            if (!int.TryParse(answer, out int scale) || scale < 0 || scale > 5)
                                context.AddFailure("Answer", $"Answer must be a number between 0 and 5 for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.MultiNomialSingleChoiceOther:
                            if (int.TryParse(answer, out int choice2))
                            {
                                if (choice2 < 0 || choice2 >= template.AnswerOptions.Count)
                                {
                                    context.AddFailure("Answer", $"Answer must be a number between 0 and {template.AnswerOptions.Count - 1} for '{template.Question}-{template.Id}'.");
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
