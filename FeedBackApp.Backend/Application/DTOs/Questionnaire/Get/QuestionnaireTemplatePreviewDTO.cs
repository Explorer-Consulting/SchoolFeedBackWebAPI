using FeedBackApp.Core.Model;

namespace Application.DTOs.Questionnaire.Get
{
    public record QuestionnaireTemplatePreviewDTO(string Title, IList<QuestionTemplate> QuestionTemplates, bool SelfEnrollmentAllowed);
}
