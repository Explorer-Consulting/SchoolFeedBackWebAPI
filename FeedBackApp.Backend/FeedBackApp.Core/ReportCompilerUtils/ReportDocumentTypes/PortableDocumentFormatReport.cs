using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.ReportDocumentTypes
{
    public sealed class PortableDocumentFormatReport : ReportDocument, IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            throw new NotImplementedException();
        }
    }
}
