using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
{

    /// <summary>
    /// Provides static factory methods for creating Excel cell objects with specified content and formatting.
    /// </summary>
    internal class ExcelCellFactory
    {
        /// <summary>
        /// Creates a text cell (InlineString) with the given style index.
        /// </summary>
        /// <param name="text">Cell text (empty string if null).</param>
        /// <param name="styleIndex">Cell format style index.</param>
        internal static Cell CreateTextCell(string? text, uint styleIndex = 0) =>
            new()
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(text ?? string.Empty)),
                StyleIndex = styleIndex
            };

        /// <summary>
        /// Creates a numeric cell (Number) using InvariantCulture formatting.
        /// </summary>
        /// <param name="value">The numeric value.</param>
        /// <param name="styleIndex">Cell format style index.</param>
        internal static Cell CreateNumberCell(double value, uint styleIndex = 0) =>
            new()
            {
                CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture)),
                DataType = CellValues.Number,
                StyleIndex = styleIndex
            };

    }
}
