using FluentValidation;
using ApplicationLayer.DataTransferObjects;

namespace ApplicationLayer.DataTransferObjectValidators;

public sealed class QuestionnaireTemplateDTOValidator : AbstractValidator<QuestionnaireTemplateDTO>
{
    public QuestionnaireTemplateDTOValidator()
    {
        RuleFor(x => x.QuestionnaireTemplateBusinessID)
            .MustBeULID();

        RuleFor(x => x.Metadata)
            .NotNull()
            .SetValidator(new QuestionnaireTemplateMetadataDTOValidator());

        RuleFor(x => x.QuestionItems)
            .NotNull().WithMessage("QuestionItems cannot be null.")
            .Must(i => i.Count > 0)
            .WithMessage("QuestionItems must not be empty.");

        RuleForEach(x => x.QuestionItems)
            .SetValidator(new QuestionItemDTOValidator());
    }
}
