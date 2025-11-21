using FeedBackApp.Core.DomainModels.QuestionTemplateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace FeedBackApp.Core.DomainModels.AssignedQuestionnaireModels
{
    public class AssignedQuestionnaireResponse
    {
        public required AssignedQuestionnaireAttributeHeader Header { get; init; }

        public required IReadOnlyCollection<QuestionTemplateAnswer> AnswerCollection { get; init; }
    }
}
