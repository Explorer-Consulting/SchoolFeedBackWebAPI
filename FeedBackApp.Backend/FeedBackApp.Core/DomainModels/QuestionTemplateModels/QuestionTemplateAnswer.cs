using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.DomainModels.QuestionTemplateModels
{
    public class QuestionTemplateAnswer
    {
        public required int Id { get; init; }

        public required QuestionTemplateType Type {get; init;}

        public required IReadOnlyCollection<string> Answer { get; init; }
    }
}
