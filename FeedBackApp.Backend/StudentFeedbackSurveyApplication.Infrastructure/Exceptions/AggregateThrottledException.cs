using StudentFeedbackSurveyApplication.Infrastructure.Exceptions;

public sealed class AggregateThrottledException : InfrastructureException
{
    public TimeSpan? RetryAfter { get; }

    public AggregateThrottledException(
        string message,
        TimeSpan? retryAfter,
        Exception innerException)
        : base(message, innerException)
    {
        RetryAfter = retryAfter;
    }
}
