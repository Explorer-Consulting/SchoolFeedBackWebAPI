using ApplicationLayer.DataTransferObjects;
using FluentValidation;

namespace ApplicationLayer.DataTransferObjectValidators
{
    public sealed class QuestionItemDTOValidator : AbstractValidator<QuestionItemDTO>
    {
        public QuestionItemDTOValidator()
        {
            RuleFor(dto => dto.QuestionID)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("QuestionID cannot be null.")
                .NotEmpty().WithMessage("QuestionID cannot be empty.")
                .Must(IsPositiveInteger)
                    .WithMessage("QuestionID must be a positive integer represented as a string.")
                    .WithErrorCode("InvalidQuestionID");

            RuleFor(dto => dto.QuestionType)
                .IsInEnum()
                    .WithMessage("QuestionType must be a valid enum value.")
                    .WithErrorCode("InvalidQuestionType");

            RuleFor(dto => dto.QuestionStatement)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("QuestionStatement cannot be null.")
                .NotEmpty().WithMessage("QuestionStatement cannot be empty.")
                .MaximumLength(500)
                    .WithMessage("QuestionStatement cannot exceed 500 characters.")
                    .WithErrorCode("InvalidQuestionStatement");

            RuleFor(dto => dto.AnswerOptions)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("AnswerOptions cannot be null.")
                .NotEmpty().WithMessage("AnswerOptions cannot be empty.")
                .Must(options => options.All(option => !string.IsNullOrWhiteSpace(option)))
                    .WithMessage("All AnswerOptions must be non-empty strings.")
                    .WithErrorCode("InvalidAnswerOptions");

            RuleFor(dto => dto.Dependencies)
                .NotNull().WithMessage("Dependencies cannot be null.")
                .NotEmpty().WithMessage("Dependencies cannot be empty.");

            RuleForEach(dto => dto.Dependencies)
                .SetValidator(new QuestionDependencyDTOValidator());
        }

        private static bool IsPositiveInteger(string? value) =>
            value is not null && int.TryParse(value, out var result) && result > 0;
    }
}
