using StudentFeedbackSurveyApplication.Domain.DomainAggregateRoots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentFeedbackSurveyApplication.Infrastructure
{
    public interface IQuestionnaireResponseRepository : IAggregateEntityRepository<QuestionnaireResponseDocument>
    {
        // ide is a specifikus dolgokat
    }
}
