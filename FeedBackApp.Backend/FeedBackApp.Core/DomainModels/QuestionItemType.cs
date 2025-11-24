using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModels
{
    public enum QuestionItemType
    {
        SingleChoice,
        MultipleChoice,
        OpenEnded,
        LikertScale,
        SingleChoiceWithOpenEnded
    }
}
