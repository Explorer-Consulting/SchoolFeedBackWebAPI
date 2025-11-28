using ApplicationLayer.DataTransferObjects;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationLayer.DataTransferObjectValidators
{
    public sealed class QuestionItemDTOValidator : AbstractValidator<QuestionItemDTO>
    {
        public QuestionItemDTOValidator()
        {
            RuleFor(dto => dto.QuestionID)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("QuestionID cannot be null.")
                .NotEmpty().WithMessage("QuestionID cannot be empty.");
                
        }
    }
}
