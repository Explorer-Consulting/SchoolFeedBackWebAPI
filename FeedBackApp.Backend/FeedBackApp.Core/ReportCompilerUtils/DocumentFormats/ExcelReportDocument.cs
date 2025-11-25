using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.Model.Enum;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatMethods;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentUtils;
using FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.Model;
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
            using var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true);
           
            // Workbook 
            var wbPart = doc.AddWorkbookPart();
            wbPart.Workbook = new Workbook();

            // Stylesheet     
            var styles = wbPart.AddNewPart<WorkbookStylesPart>();
            styles.Stylesheet = ExcelStylesheetBuilder.BuildStylesheet();
            styles.Stylesheet.Save();

            var sheets = wbPart.Workbook.AppendChild(new Sheets());

            // Creating sheet models from domain components
            var sheetModels = ExcelSheetModelBuilder.BuildSheetsModelsFromComponents(
               ReportComponents.OfType<IReportComponent>());

            // If sheetModels is empty, we create an "Empty" sheet
            if (!sheetModels.Any())
            {
                var emptyBlocks = new List<(List<string> Main, List<string> Opts)>
                    {
                        (new List<string>{ "—" }, new List<string>())
                    };

                // Create the "Empty" sheet
                ExcelWorksheetRenderer.RenderWorksheet(
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
                        // Generate a unique sheet name
                        var sheetName = NameUtils.MakeUniqueName(model.DisplayName, usedNames, invalidChars);

                        // Calculate layout configuration for the sheet
                        var layout = ExcelSheetLayoutCalculator.CalculateLayout(model);

                        // Normalize blocks for consistent width based on layout
                        var normalized = ExcelSheetLayoutCalculator.NormalizeBlocks(model.Blocks, layout);

                        // Create the sheet
                        ExcelWorksheetRenderer.RenderWorksheet(
                            wbPart, sheets, sheetName,
                            layout.HeaderRow.ToList(),
                            normalized,
                            sheetId++,
                            layout.MaxAnswerColumns,
                            layout.MaxOptionColumns
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
