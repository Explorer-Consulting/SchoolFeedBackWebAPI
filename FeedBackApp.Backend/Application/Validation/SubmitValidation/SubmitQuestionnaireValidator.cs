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

            RuleFor(x => x)
                .Custom((dto, context) =>
                {
                    var q19 = dto.QuestionnaireResult.FirstOrDefault(r => r.QuestionId == "q18");
                    var q20 = dto.QuestionnaireResult.FirstOrDefault(r => r.QuestionId == "q19");

                    if (q19 == null)
                    {
                        if (q20 == null || string.IsNullOrWhiteSpace(q20.Answer))
                        {
                            context.AddFailure("QuestionnaireResult",
                                "Question 20 must be answered unless Question 19 is answered with option 3.");
                        }
                        return;
                    }

                    if (q19.Answer == "3")
                    {
                        return;
                    }

                    if (q20 == null || string.IsNullOrWhiteSpace(q20.Answer))
                    {
                        context.AddFailure("QuestionnaireResult",
                            "Question 20 must be answered unless Question 19 is answered with option 3.");
                    }
                });
        }
    }
}
