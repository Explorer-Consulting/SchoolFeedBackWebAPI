namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public sealed class AggregateTransientFailureException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
