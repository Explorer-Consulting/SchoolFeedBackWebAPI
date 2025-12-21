
namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    internal sealed class AggregateRetrievalFailedException : InfrastructureException
    {
        public AggregateRetrievalFailedException(string message)
            : base(message)
        {
        }

        public AggregateRetrievalFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
