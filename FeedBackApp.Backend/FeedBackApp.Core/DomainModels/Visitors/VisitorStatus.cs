using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.DomainModels.Visitors
{
    public enum VisitorStatus
    {
        Internal, // a user that is part of the system from the beginning of the questionnaire publishing process.
        Prospect, // a user that has shown interest for a specific questionnaire and is being considered for participation (someone with no precompiled questionnaire, self-assigner).
        Assignor // someon who manages questionnaire assignments and publishing but does not directly participate.
    }
}
