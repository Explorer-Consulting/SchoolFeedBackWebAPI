
namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
{
    internal static class BlockUtils
    {
        public static List<(List<string> Main, List<string> Opts)> NormalizeBlocks(
            IReadOnlyList<(List<string> Main, List<string> Opts)> blocks,
            int mainCols,
            int optionCols,
            int totalCols)
        {
            var normalized = new List<(List<string> Main, List<string> Opts)>(blocks.Count);

            foreach (var blk in blocks)
            {
                var m = new List<string>(blk.Main);
                var o = new List<string>(blk.Opts ?? new List<string>());

                PadTo(m, mainCols);
                PadTo(o, optionCols);
                PadTo(m, totalCols);
                PadTo(o, totalCols);

                normalized.Add((m, o));
            }

            return normalized;
        }
        #region think it through
        private static void PadTo(List<string> list, int size)
        {
            while (list.Count < size)
                list.Add(string.Empty);
        }
        #endregion
    }

}
