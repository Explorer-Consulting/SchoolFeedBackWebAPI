using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportDocumentTypes
{
    public class ReportDocument  
    {
        public required ReportMetadata Metadata { get; init; }
        public ReportDocument() { }
    }
}
