using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;

namespace StudentFeedbackSurveyApplication.Infrastructure
{
    public interface IQuestionnaireTemplateRepository : IAggregateEntityRepository<QuestionnaireTemplateDocument>
    {
        // ide rakom a specifikus lekerdezeseket majd...
    }
}
