using DocumentFormat.OpenXml.Spreadsheet;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentUtils
{
    internal class ExcelBuildStylesheetUtils
    {

        /// <summary>
        /// Cell styles for Excel document generation.
        /// </summary>
        internal static class CellStyles
        {
            public const uint Default = 0;
            public const uint Header = 1;
            public const uint Text = 2;
            public const uint Options = 3;
            public const uint Numeric = 4;
        }


        /// <summary>
        /// Defines the base stylesheet: fonts, fills, borders, and cell formats.
        /// </summary>
        /// <remarks>
        /// Fonts: normal, bold (header), italic (options).
        /// Fills: None, Gray125, light grays, and bluish fill (numeric background).
        /// Borders: thin border.
        /// CellFormats:
        ///  - 0: default
        ///  - 1: header (bold + gray background + centered)
        ///  - 2: text (bordered, wrap enabled)
        ///  - 3: options (italic + light background)
        ///  - 4: numbers (bluish background, right aligned)
        /// </remarks>
        internal static Stylesheet BuildStylesheet()
        {

            var fonts = CreateFonts();

            var fills = CreateFills();

            var borders = CreateBorders();

            var cellStyleFormats = new CellStyleFormats(new CellFormat());

            var cellFormats = CreateCellFormats();

          
            return new Stylesheet
            {
                Fonts = fonts,
                Fills = fills,
                Borders = borders,
                CellStyleFormats = cellStyleFormats,
                CellFormats = cellFormats
            };
        }

        private static Fonts CreateFonts() =>
            new(
                new Font(),
                new Font(new Bold()),
                new Font(new Italic())
            );

        private static Fills CreateFills() => new(
                new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = "FFD9D9D9" }) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = "FFF2F2F2" }) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = "FFE6F0FF" }) { PatternType = PatternValues.Solid })
            );

        private static Borders CreateBorders()
        {
            var borderThin = new Border(
                new LeftBorder { Style = BorderStyleValues.Thin },
                new RightBorder { Style = BorderStyleValues.Thin },
                new TopBorder { Style = BorderStyleValues.Thin },
                new BottomBorder { Style = BorderStyleValues.Thin },
                new DiagonalBorder()
            );
            return new Borders(new Border(), borderThin);
        }

        private static CellFormats CreateCellFormats() =>
            new(
                new CellFormat(), // 0: default
                new CellFormat // 1: header
                {
                    FontId = 1,
                    FillId = 2,
                    BorderId = 1,
                    ApplyFont = true,
                    ApplyFill = true,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Horizontal = HorizontalAlignmentValues.Center,
                        Vertical = VerticalAlignmentValues.Center,
                        WrapText = true
                    }
                },
                new CellFormat // 2: text data
                {
                    FontId = 0,
                    FillId = 0,
                    BorderId = 1,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Vertical = VerticalAlignmentValues.Center,
                        WrapText = true
                    }
                },
                new CellFormat // 3: options row
                {
                    FontId = 2,
                    FillId = 3,
                    BorderId = 1,
                    ApplyFont = true,
                    ApplyFill = true,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Vertical = VerticalAlignmentValues.Center,
                        WrapText = true
                    }
                },
                new CellFormat // 4: numeric data
                {
                    FontId = 0,
                    FillId = 4,
                    BorderId = 1,
                    ApplyFill = true,
                    ApplyBorder = true,
                    Alignment = new Alignment
                    {
                        Horizontal = HorizontalAlignmentValues.Right,
                        Vertical = VerticalAlignmentValues.Center
                    }
                }


            );

    }
}
