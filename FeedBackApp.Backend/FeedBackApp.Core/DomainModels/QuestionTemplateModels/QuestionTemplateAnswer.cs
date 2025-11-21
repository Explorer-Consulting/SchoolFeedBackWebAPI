using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.DomainModels.QuestionTemplateModels
{
    public class QuestionTemplateAnswer
    {
        /// <summary>
        /// the number of the question inside a questionnaire template
        /// </summary>
        public required int QuestionNumberInQuestionnaireTemplate { get; init; }

        public required QuestionTemplateType Type {get; init;}

        public required IReadOnlyCollection<string> Answer { get; init; }
    }
}
