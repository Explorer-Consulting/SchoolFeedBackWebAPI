namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public sealed class AggregateStorageException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
