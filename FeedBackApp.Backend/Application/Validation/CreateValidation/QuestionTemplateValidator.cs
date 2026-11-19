using Application.DTOs.Questionnaire;
using FeedBackApp.Core.Model.Enum;
using FluentValidation;

namespace Application.Validation.CreateValidation
{
    public class QuestionTemplateValidator : AbstractValidator<QuestionTemplateDTO>
    {
        public QuestionTemplateValidator()
        {
            RuleFor(dto => dto.Category).NotEmpty().WithMessage("Category can not be empty");
            RuleFor(dto => dto.Question).NotEmpty().WithMessage("Question text can not be empty")
                .MaximumLength(500).WithMessage("Question can not be longer than 500 characters: {PropertyValue}");
            RuleFor(dto => dto.Type)
                .Must(type => type != 0).WithMessage("Question type is required")
                .IsInEnum().WithMessage("Invalid question type: {PropertyValue}");
            When(dto => dto.Type == QuestionType.MultinomialSingleChoice
                     || dto.Type == QuestionType.MultipleChoice, () =>
                     {
                         RuleFor(dto => dto.AnswerOptions)
                             .NotEmpty().WithMessage("Answer options are required for single- or multiple-choice questions questions")
                             .Must(options => options != null && options.Any(o => !string.IsNullOrWhiteSpace(o)))
                                 .WithMessage("Answer options must contain at least one non-empty option for single- or multiple-choice questions")
                             .Must(options => options!.All(o => !string.IsNullOrWhiteSpace(o)))
                                 .WithMessage("Answer options cannot contain empty values for single- or multiple-choice questions");
                     });
            When(dto => dto.Type is QuestionType.DropDown or QuestionType.FilteredDropDown, () =>
            {
                RuleFor(dto => dto.DropDownItems)
                    .Cascade(CascadeMode.Stop)

                    .NotEmpty()
                    .WithMessage("Dropdown items are required.")

                    .Must(items => items.All(item => item != null))
                    .WithMessage("Dropdown items cannot contain null elements.")

                    .Must(items => items.All(item =>
                        !string.IsNullOrWhiteSpace(item.Id)))
                    .WithMessage("Dropdown item IDs cannot be empty.")

                    .Must(items => items.All(item =>
                        !string.IsNullOrWhiteSpace(item.Label)))
                    .WithMessage("Dropdown item labels cannot be empty.")

                    .Must(items =>
                        items.Select(item => item.Id)
                            .Distinct(StringComparer.Ordinal)
                            .Count() == items.Count)
                    .WithMessage("Dropdown item IDs must be unique.");
            });

            When(dto => dto.Type is QuestionType.FilteredDropDown, () => {
                RuleFor(dto => dto.ParentQuestionId)
                    .NotEmpty().WithMessage("Filtered dropdown requires a parent question.");

                RuleFor(dto => dto.DropDownItems)
                    .Must(items => items.All(item => !string.IsNullOrWhiteSpace(item.ParentId)))
                    .WithMessage("Every filtered dropdown item must have a parent item ID.");
            });
        }
    }
}
