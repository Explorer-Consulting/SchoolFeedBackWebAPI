using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;


namespace StudentFeedbackSurveyApplication.Infrastructure
{
    public interface IUserRepository : IAggregateEntityRepository<User>
    {
        // ide is szinten a specifikusak
    }
}
