using Core.Interfaces;
using FeedBackApp.Core.DomainModels.QuestionTemplateModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Core.DomainModels.AssignedQuestionnaireModels
{
    public sealed class AssignedQuestionnaireResponse : IAggregateRoot
    {
        public required AssignedQuestionnaireAttributeHeader Header { get; init; }

        public required IReadOnlyCollection<QuestionTemplateAnswer> AnswerCollection { get; init; }
    }
}
