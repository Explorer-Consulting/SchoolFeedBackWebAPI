using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IReportService
    {
        // these methods depends on performance/customer needs and can be changed in the future
        Task Deliver(string EmailAddress);
        Task Deliver(/*implementation-dependent*/);
    }
}
