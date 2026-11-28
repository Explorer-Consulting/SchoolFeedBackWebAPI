using Application.DTOs.Evaluation;
using FeedBackApp.Core.Model;
using FeedBackApp.Core.Model.Enum;
using FluentValidation;

namespace Application.Validation.SubmitValidation
{
    public class QuestionSubmitValidator : AbstractValidator<QuestionResultDTO>
    {
        public QuestionSubmitValidator(IList<QuestionTemplate> templates)
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

                    if (answer == string.Empty)
                    {
                        return;
                    }

                    switch (template.Type)
                    {
                        case QuestionType.OpenEnded:
                            if (string.IsNullOrWhiteSpace(answer))
                                context.AddFailure("Answer", $"Answer cannot be empty for '{template.Question}-{template.Id}'.");
                            else if (answer.Length < 20)
                                context.AddFailure("Answer", $"Answer cannot be less than 20 characters for '{template.Question}-{template.Id}'.");
                            else if (answer.Length > 300)
                                context.AddFailure("Answer", $"Answer cannot be longer than 300 characters for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.MultinomialSingleChoice:
                            if (!int.TryParse(answer, out int singleChoice) || singleChoice < 1 || singleChoice > template.AnswerOptions.Count)
                                context.AddFailure("Answer", $"Answer must be a number between 1 and {template.AnswerOptions.Count} for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.MultipleChoice:
                            var parts = answer.Split('-', StringSplitOptions.RemoveEmptyEntries);
                            if (template.MaxSelections is int max && parts.Length > max)
                            {
                                context.AddFailure(
                                    "Answer",
                                    $"At most {max} options may be selected for '{template.Question}-{template.Id}'.");
                                break;
                            }
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
                                context.AddFailure("Answer", $"Answer must be a number between 1 and 5 for '{template.Question}-{template.Id}'.");
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

                        case QuestionType.Number:
                            if (!AnswerFormats.TryParseNumber(answer, out _))
                                context.AddFailure("Answer", $"Answer must be a valid number (e.g. 12 or 3.5) for '{template.Question}-{template.Id}'.");
                            break;

                        case QuestionType.Date:
                            if (!AnswerFormats.TryParseDate(answer, out _))
                                context.AddFailure("Answer", $"Answer must be a date in {AnswerFormats.DateFormat} format for '{template.Question}-{template.Id}'.");
                            break;
                        case QuestionType.FilteredDropDown:
                    case QuestionType.DropDown:
                        {
                            var matchesItem = template.DropDownItems.Any(item =>
                                string.Equals(item.Id, answer, StringComparison.Ordinal));

                            if (!matchesItem)
                            {
                                if (!template.AllowOther || string.IsNullOrWhiteSpace(answer))
                                {
                                    context.AddFailure(
                                        "Answer",
                                        $"Answer must match an existing dropdown item ID " +
                                        $"for '{template.Question}-{template.Id}'.");
                                }
                            }
                            break;
                        }
                    }
                });
        }
    }
}
