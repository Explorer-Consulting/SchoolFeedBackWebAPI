namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public sealed class AggregateConflictException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
