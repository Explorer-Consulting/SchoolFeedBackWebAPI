using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentFeedbackSurveyApplication.Infrastructure.Exceptions
{
    public class AggregateCreationFailedException(string message, Exception ex) : InfrastructureException(message, ex)
    {
    }
}
