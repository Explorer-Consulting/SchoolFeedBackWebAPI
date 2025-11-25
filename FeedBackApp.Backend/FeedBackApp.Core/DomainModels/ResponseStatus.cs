using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DomainModels
{
    public enum ResponseStatus
    {
        Pending, // the participant has not opened or interacted with the questionnaire yet.
        InProgress, // the participant started the questionnaire but has not submitted it yet.
        Submitted, // the participant submitted the questionnaire successfully.
        Invalidated, // the response was manually or automatically invalidated by the system or an administrator.
        Verified // an administrator or instructor has reviewed and validated the submitted response.
    }
}
