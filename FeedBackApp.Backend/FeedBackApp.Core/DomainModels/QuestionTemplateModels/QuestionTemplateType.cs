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
        SingleChoice,
        Hibrid,
        MultipleChoice,
        OpenEnded,
        LikertScale
    }
}
