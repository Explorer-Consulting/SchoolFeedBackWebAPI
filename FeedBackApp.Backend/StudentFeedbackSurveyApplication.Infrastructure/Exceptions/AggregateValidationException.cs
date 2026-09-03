namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public sealed class AggregateValidationException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
