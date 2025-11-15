using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.DomainModels.QuestionTemplateModels
{
    public class QuestionTemplateDependency
    {
        public required int DependencyId { get; init; }
        public required IReadOnlyCollection<int> DependencyOptions { get; init; }
    }
}
