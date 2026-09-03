namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public sealed class AggregateForbiddenException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
