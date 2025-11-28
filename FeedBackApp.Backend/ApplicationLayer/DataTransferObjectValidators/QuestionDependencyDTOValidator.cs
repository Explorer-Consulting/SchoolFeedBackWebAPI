using ApplicationLayer.DataTransferObjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DataTransferObjectValidators
{
    public sealed class QuestionDependencyDTOValidator : AbstractValidator<QuestionDependencyDTO>
    {
        public QuestionDependencyDTOValidator()
        {
            RuleFor(dto => dto.QuestionID)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("QuestionID cannot be null.")
                .NotEmpty().WithMessage("QuestionID cannot be empty.")
                .Must(IsPositiveInteger)
                .WithMessage("QuestionID must be a positive integer represented as a string.")
                .WithErrorCode("InvalidQuestionID");

            RuleFor(dto => dto.ExpectedAnswerIndexes)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("ExpectedAnswerIndexes cannot be null.")
                .NotEmpty().WithMessage("ExpectedAnswerIndexes cannot be empty.")
                .Must(indexes => indexes.All(IsPositiveInteger))
                .WithMessage("All ExpectedAnswerIndexes must be positive integers represented as strings.")
                .WithErrorCode("InvalidExpectedAnswerIndexes");
        }

        private static bool IsPositiveInteger(string? value) => value is not null && int.TryParse(value, out int result) && result > 0;
    }
}
