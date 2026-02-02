
namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public sealed class AggregateNotFoundException : InfrastructureException
    {
        public AggregateNotFoundException(string message)
            : base(message)
        {
        }

        public AggregateNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
