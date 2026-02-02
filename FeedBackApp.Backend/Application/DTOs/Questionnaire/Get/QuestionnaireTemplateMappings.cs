using Application.DTOs.Questionnaire.Get;
using FeedBackApp.Core.Model;

namespace Application.Extensions.QuestionnaireExtensions
{
    public static class QuestionnaireTemplatePreviewMappings
    {
        public static QuestionnaireTemplatePreviewDTO ToPreviewDto(
            this QuestionnaireTemplate model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new QuestionnaireTemplatePreviewDTO(
                SelfEnrollmentAllowed: model.IsSelfOptInEnabled,
                Title: model.Title,
                QuestionTemplates: model.QuestionTemplates
            );
        }
    }
}
