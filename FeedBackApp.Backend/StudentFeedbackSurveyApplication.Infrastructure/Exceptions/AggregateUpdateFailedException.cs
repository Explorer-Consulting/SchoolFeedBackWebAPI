namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public class AggregateUpdateFailedException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
