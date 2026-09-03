namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public sealed class AggregateUnauthorizedException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
