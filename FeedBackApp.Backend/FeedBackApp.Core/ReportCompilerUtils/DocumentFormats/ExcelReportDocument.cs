using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatMethods;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentUtils;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.ReportComponentsModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Document class that generates the Administrator Excel report.
    /// Based on the DataSource of the report components found in <see cref="ReportComponent"/>,
    /// it creates separate worksheets per type (Likert, SingleChoice, SingleChoice+Other, MultipleChoice, OpenEnded),
    /// with automatic column widths, base styles, and a frozen header row.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>DataSource</c> property of each component is read via reflection, and the switch
    /// cases below convert it into normalized rows based on the type. Each question forms a “block”:
    /// a main row (Main) and optionally an options row (Opts). Numeric values are written as Number cells,
    /// text values as InlineString cells.
    /// </para>
    /// <para>
    /// If <see cref="ReportComponents"/> is empty, an “Empty” sheet is created as an indicator.
    /// The generated document is built in memory, and <see cref="RenderDocument"/> returns a byte array.
    /// </para>
    /// </remarks>
    public sealed class ExcelReportDocument(ReportMetadata metadata, Recipient? recipient = null)
        : ReportDocument(metadata, recipient)
    {
        /// <summary>
        /// Builds the complete .xlsx file: workbook, stylesheet, sheets, rows, and columns.
        /// </summary>
        /// <returns>The generated Excel document bytes.</returns>
        /// <remarks>
        /// Steps:
        /// 1) Create Workbook and Stylesheet.
        /// 2) Traverse components and group blocks by sheet type.
        /// 3) Record maximum counts of answers/options for within-sheet normalization.
        /// 4) Create worksheets with consistent row widths.
        /// 5) Save the workbook and return the byte[].
        /// </remarks>
        public override Task<byte[]> RenderDocument()
        {
            using var ms = new MemoryStream();
            using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                // Workbook and stylesheet
                var wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                var styles = wbPart.AddNewPart<WorkbookStylesPart>();
                styles.Stylesheet = ExcelStylesheetBuilder.BuildStylesheet();
                styles.Stylesheet.Save();

                var sheets = wbPart.Workbook.AppendChild(new Sheets());

                // creating sheet models from domain components
                var sheetModels = ExcelReportBuilder.BuildSheets(
                   ReportComponents.OfType<IReportComponent>());

                // if sheetModels is empty, we create an "Empty" sheet
                if (!sheetModels.Any())
                {
                    var emptyBlocks = new List<(List<string> Main, List<string> Opts)>
                    {
                        (new List<string>{ "—" }, new List<string>())
                    };

                    ExcelCreateSheet.CreateSheet(
                        wbPart, sheets, "Üres",
                        header: ["Kérdés"],
                        blocks: emptyBlocks,
                        explicitSheetId: null,
                        maxAns: 0, maxOpts: 0
                    );
                }
                else
                {
                    // We have data: create a separate worksheet for each sheet type
                    uint sheetId = 1;
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // invalid characters for Excel sheet names
                    var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };

                    foreach (var model in sheetModels)
                    {
                        {
                            var sheetName = NameUtils.MakeUniqueName(model.RawName, usedNames, invalidChars);

                            // extracting model data
                            var maxAns = model.MaxAns;
                            var maxOpts = model.MaxOpts;
                            var blocks = model.Blocks;

                            // Header for the main table
                            var header = new List<string> { "Kérdés" };


                            if (sheetName.Equals("Likert-skála", StringComparison.OrdinalIgnoreCase))
                            {
                                for (int i = 0; i < maxAns; i++) header.Add(string.Empty);
                                header.Add("Értékek jelentése");
                            }
                            else
                            {
                                for (int i = 0; i < maxAns; i++) header.Add(string.Empty);
                            }

                            // Total width: max(main, options)
                            var mainCols = 1 + maxAns + (sheetName.Equals("Likert-skála", StringComparison.OrdinalIgnoreCase) ? 1 : 0);
                            var optionCols = 1 + maxOpts;
                            var totalCols = Math.Max(mainCols, optionCols);
                            while (header.Count < totalCols) header.Add(string.Empty);

                            // Normalize blocks to totalCols width
                            var normalized = new List<(List<string> Main, List<string> Opts)>(blocks.Count);
                            foreach (var blk in blocks)
                            {
                                var m = new List<string>(blk.Main);
                                var o = new List<string>(blk.Opts ?? new List<string>());

                                while (m.Count < mainCols) m.Add(string.Empty);
                                while (o.Count < optionCols) o.Add(string.Empty);
                                while (m.Count < totalCols) m.Add(string.Empty);
                                while (o.Count < totalCols) o.Add(string.Empty);

                                normalized.Add((m, o));
                            }

                            ExcelCreateSheet.CreateSheet(
                                wbPart, sheets, sheetName,
                                header, normalized, sheetId++,
                                maxAns, maxOpts
                            );
                        }
                    }

                    wbPart.Workbook.Save();
                }

                Data = ms.ToArray();
                return Task.FromResult(Data);
            }
        }
    }
}
