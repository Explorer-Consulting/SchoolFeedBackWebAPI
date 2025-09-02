using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.DomainMetadata
{
    public sealed record AdministratorMetadata(string EmailAddress, string FirstName, string LastName);
}
