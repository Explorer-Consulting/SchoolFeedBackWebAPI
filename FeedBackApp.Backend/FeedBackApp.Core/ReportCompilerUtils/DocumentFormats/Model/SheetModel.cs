
namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.model
{
    public sealed class SheetModel
    {
        public string RawName { get; init; } = "";
        public string SheetName { get; set; } = "";
        public List<string> Header { get; init; } = new();
        public List<(List<string> Main, List<string> Opts)> Blocks { get; init; } = new();

        #region Clarify, please, why we need this
        public int MaxAns { get; init; }
        #endregion
        public int MaxOpts { get; init; }
    }
}
