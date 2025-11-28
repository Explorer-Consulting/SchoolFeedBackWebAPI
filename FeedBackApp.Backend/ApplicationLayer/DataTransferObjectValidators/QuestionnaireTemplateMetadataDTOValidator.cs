using System;
using FluentValidation;
using ApplicationLayer.DataTransferObjects;

namespace ApplicationLayer.DataTransferObjectValidators
{
    public sealed class QuestionnaireTemplateMetadataDTOValidator
        : AbstractValidator<QuestionnaireTemplateMetadataDTO>
    {
        public QuestionnaireTemplateMetadataDTOValidator()
        {
            RuleFor(dto => dto.Title)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Title cannot be null.")
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.")
                .WithErrorCode("InvalidTitle");

            RuleFor(dto => dto.Description)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Description cannot be null.")
                .NotEmpty().WithMessage("Description cannot be empty.")
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.")
                .WithErrorCode("InvalidDescription");

            RuleFor(dto => dto.CreatedAt)
                .Cascade(CascadeMode.Stop)
                .NotEqual(default(DateTimeOffset))
                    .WithMessage("CreatedAt must be set.")
                .LessThanOrEqualTo(_ => DateTimeOffset.UtcNow)
                    .WithMessage("CreatedAt cannot be in the future.")
                .WithErrorCode("InvalidCreatedAt");

            RuleFor(dto => dto.ActivationDate)
                .Cascade(CascadeMode.Stop)
                .NotEqual(default(DateTimeOffset))
                    .WithMessage("ActivationDate must be set.")
                .GreaterThanOrEqualTo(dto => dto.CreatedAt)
                    .WithMessage("ActivationDate cannot be earlier than CreatedAt.")
                .WithErrorCode("InvalidActivationDate");

            RuleFor(dto => dto.ExpirationDate)
                .Cascade(CascadeMode.Stop)
                .NotEqual(default(DateTimeOffset))
                    .WithMessage("ExpirationDate must be set.")
                .GreaterThan(dto => dto.ActivationDate)
                    .WithMessage("ExpirationDate must be later than ActivationDate.")
                .WithErrorCode("InvalidExpirationDate");
        }
    }
}
