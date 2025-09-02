using FeedBackApp.Core.ReportCompilerUtils.ReportDocumentTypes;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.Renderers
{
    public class EvaluationReportRenderer
    {
        public async static IAsyncEnumerable<ReportDocument> RenderReportAsync<TRecipient, TDocumentFormat>(Dictionary<RecipientType, DocumentFormatType> data)
        {
            ArgumentNullException.ThrowIfNull(data);

            foreach(var member in data)
            {
                //csinal valamit
                ReportDocument? v = null;
                yield return v;

            }
        }
    }
}
