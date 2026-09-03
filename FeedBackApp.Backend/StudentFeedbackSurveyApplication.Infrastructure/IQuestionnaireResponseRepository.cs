using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;

namespace StudentFeedbackSurveyApplication.Infrastructure
{
    public interface IQuestionnaireResponseRepository : IAggregateEntityRepository<QuestionnaireResponseDocument>
    {
        // ide is a specifikus dolgokat
    }
}
