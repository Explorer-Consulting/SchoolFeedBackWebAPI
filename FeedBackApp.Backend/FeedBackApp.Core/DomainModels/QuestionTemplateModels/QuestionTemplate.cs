using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.DomainModels.QuestionTemplateModels
{
    public abstract class QuestionTemplate
    {
        public required int Id { get; init; }
        public required QuestionTemplateType TemplateType { get; init; }

        public AnswerOptions<string>? Options { get; init; }
        public string? Category { get; set; }
        public required string Statement { get; init; }

        public IReadOnlyCollection<QuestionTemplateDependency>? Dependencies { get; init; }
        public string? Description { get; set; }

    }
}
