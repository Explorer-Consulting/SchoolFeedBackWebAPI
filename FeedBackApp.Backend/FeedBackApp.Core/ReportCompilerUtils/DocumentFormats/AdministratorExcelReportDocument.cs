using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    public sealed class AdministratorExcelReportDocument(ReportMetadata metadata, Recipient recipient) : ReportDocument(metadata, recipient)
    {
        public override byte[] RenderDocument()
        {
            throw new NotImplementedException();
        }
    }
}
