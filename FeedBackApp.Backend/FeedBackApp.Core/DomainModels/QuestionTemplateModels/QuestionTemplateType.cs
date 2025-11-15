using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.DomainModels.QuestionTemplateModels
{
    public enum QuestionTemplateType
    {
        Unknown,
        SingleChoiceQuestionTemplate,
        HibridQuestionTemplate,
        MultipleChoiceQuestionTemplate,
        OpenEndedQuestionTemplate,
        LikertScaleQuestionTemplate
    }
}
