namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public class AggregateCreationFailedException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
