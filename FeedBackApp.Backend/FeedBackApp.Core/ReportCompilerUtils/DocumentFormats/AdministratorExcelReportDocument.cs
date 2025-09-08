using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using FeedBackApp.Core.ReportCompilerUtils.DomainMetadata;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels;
using FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels.StatisticalEvaluationUtilityModels;
using System.Globalization;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats
{
    /// <summary>
    /// Adminisztrátori Excel-riportot készítő dokumentumosztály.
    /// A <see cref="ReportComponents"/> gyűjteményben található riport-komponensek DataSource-a alapján
    /// típusonként külön munkalapokat hoz létre (Likert, SingleChoice, SingleChoice+Other, MultipleChoice, OpenEnded),
    /// automatikus oszlopszélességgel, alap stílusokkal és fagyasztott fejléccel.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A komponensek <c>DataSource</c> tulajdonságát reflexióval olvassa ki, és a típus alapján a
    /// lenti switch-ágak alakítják normál sorokká. Minden kérdés egy "blokk": egy fő sor (Main)
    /// és opcionálisan egy opciós sor (Opts). A numerikus értékek Number cellaként, a szövegek
    /// InlineString-ként kerülnek a munkalapra.
    /// </para>
    /// <para>
    /// Ha a <see cref="ReportComponents"/> üres, egy "Empty" lap készül jelzésként.
    /// A generált dokumentum memóriában jön létre, a <see cref="RenderDocument"/> byte tömböt ad vissza.
    /// </para>
    /// </remarks>
    public sealed class AdministratorExcelReportDocument(ReportMetadata metadata, Recipient? recipient = null)
        : ReportDocument(metadata, recipient)
    {
        /// <summary>
        /// Elkészíti a teljes .xlsx fájlt: workbook, stíluslap, lapok, sorok és oszlopok.
        /// </summary>
        /// <returns>A kész Excel dokumentum bájtjai.</returns>
        /// <remarks>
        /// Lépések:
        /// 1) Workbook és Stylesheet létrehozása.
        /// 2) Komponensek bejárása, blokkok csoportosítása laptípus szerint.
        /// 3) Maximális válasz/opszió számok feljegyzése a lapon belüli normalizáláshoz.
        /// 4) Munkalapok létrehozása egységes szélességű sorokkal.
        /// 5) Workbook mentése és a byte[] visszaadása.
        /// </remarks>
        public override Task<byte[]> RenderDocument()
        {
            using var ms = new MemoryStream();
            using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                // Workbook és stíluslap
                var wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();

                var styles = wbPart.AddNewPart<WorkbookStylesPart>();
                styles.Stylesheet = BuildStylesheet();
                styles.Stylesheet.Save();

                var sheets = wbPart.Workbook.AppendChild(new Sheets());

                // Laptípus -> blokkok (Main: kérdés+válaszok, Opts: opciók sor)
                var blocksBySheet = new Dictionary<string, List<(List<string> Main, List<string> Opts)>>(StringComparer.OrdinalIgnoreCase);
                var maxAnsBySheet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                var maxOptsBySheet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                // Lokális segédfüggvény: blokk felvétele a megadott laphoz
                void AddBlock(string sheet, IEnumerable<string> main, IEnumerable<string>? opts = null)
                {
                    if (!blocksBySheet.TryGetValue(sheet, out var list))
                        blocksBySheet[sheet] = list = new();
                    list.Add(
                        (main.Select(x => x ?? string.Empty).ToList(),
                         opts is null ? [] : opts.Select(x => x ?? string.Empty).ToList())
                    );
                }

                // Komponensek bejárása, DataSource vizsgálata és blokkolás
                foreach (var comp in ReportComponents)
                {
                    var ds = comp.GetType().GetProperty("DataSource")?.GetValue(comp);
                    if (ds is null) continue;

                    switch (ds)
                    {
                        case LikertScaleEvaluationData l:
                            {
                                var main = new List<string> { l.QuestionStatement };
                                main.AddRange(l.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));
                                main.Add(l.ValueMeanings ?? string.Empty);

                                AddBlock("Likert", main);
                                UpdateMax(maxAnsBySheet, "Likert", l.Answers.Length);
                                break;
                            }

                        case SingleChoiceEvaluationData s when s.Type == SingleChoice.REGULAR:
                            {
                                var main = new List<string> { s.QuestionStatement };
                                main.AddRange(s.QuestionOptionAnswers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                                var opts = new List<string> { "Options" };
                                for (int i = 0; i < s.QuestionOptions.Length; i++)
                                    opts.Add($"{i + 1} = {s.QuestionOptions[i]}");

                                AddBlock("SingleChoice", main, opts);
                                UpdateMax(maxAnsBySheet, "SingleChoice", s.QuestionOptionAnswers.Length);
                                UpdateMax(maxOptsBySheet, "SingleChoice", s.QuestionOptions.Length);
                                break;
                            }

                        case SingleChoiceEvaluationData s:
                            {
                                var main = new List<string> { s.QuestionStatement };
                                if (s.QuestionOptionAnswers.Length > 0)
                                    main.AddRange(s.QuestionOptionAnswers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                                // Elsődleges blokk: mindig legyen egy Main sor
                                var blocks = new List<(List<string> Main, List<string> Opts)>
                                {
                                    (main, new List<string>()) // itt az Opts üres
                                };

                                // Szöveges válaszok külön sorokban
                                if (!s.QuestionOpenAnswers.IsDefaultOrEmpty && s.QuestionOpenAnswers.Length > 0)
                                {
                                    foreach (var ans in s.QuestionOpenAnswers)
                                    {
                                        blocks.Add((new List<string>(), new List<string> { "TextAnswer", ans }));
                                    }
                                    UpdateMax(maxOptsBySheet, "SingleChoice+Other", 2); // 2 oszlop: címke + válasz
                                }

                                // Előre definiált opciók külön sorokban
                                if (!s.QuestionOptions.IsDefaultOrEmpty && s.QuestionOptions.Length > 0)
                                {
                                    int idx = 1;
                                    foreach (var opt in s.QuestionOptions)
                                    {
                                        blocks.Add((new List<string>(), new List<string> { "Option", $"{idx++} = {opt}" }));
                                    }
                                    UpdateMax(maxOptsBySheet, "SingleChoice+Other", 2); // 2 oszlop: címke + opció
                                }

                                // Felvétel a laphoz
                                if (!blocksBySheet.TryGetValue("SingleChoice+Other", out var list))
                                    blocksBySheet["SingleChoice+Other"] = list = new();
                                list.AddRange(blocks);

                                // Numerikus oszlopok max száma
                                UpdateMax(maxAnsBySheet, "SingleChoice+Other", s.QuestionOptionAnswers.Length);

                                break;
                            }

                        case MultipleChoiceEvaluationData m:
                            {
                                var main = new List<string> { m.QuestionStatement };
                                main.AddRange(m.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                                var opts = new List<string> { "Options" };
                                for (int i = 0; i < m.AnswerOptions.Length; i++)
                                    opts.Add($"{i + 1} = {m.AnswerOptions[i]}");

                                AddBlock("MultipleChoice", main, opts);
                                UpdateMax(maxAnsBySheet, "MultipleChoice", m.Answers.Length);
                                UpdateMax(maxOptsBySheet, "MultipleChoice", m.AnswerOptions.Length);
                                break;
                            }

                        case OpenEndedEvaluationData o:
                            {
                                var main = new List<string> { o.QuestionStatement };
                                main.AddRange(o.Answers);

                                AddBlock("OpenEnded", main);
                                UpdateMax(maxAnsBySheet, "OpenEnded", o.Answers.Length);
                                break;
                            }
                    }
                }

                // Ha nincs adat, készítsünk egy "Empty" lapot
                if (blocksBySheet.Count == 0)
                {
                    var emptyBlocks = new List<(List<string> Main, List<string> Opts)>
                    {
                        (new List<string>{ "—" }, new List<string>())
                    };

                    CreateSheet(
                        wbPart, sheets, "Empty",
                        header: ["Question"],
                        blocks: emptyBlocks,
                        explicitSheetId: null,
                        maxAns: 0, maxOpts: 0
                    );
                }
                else
                {
                    // Van adat: minden laptípusra külön munkalapot készítünk
                    uint sheetId = 1;
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    foreach (var (rawName, blocks) in blocksBySheet)
                    {
                        var sheetName = MakeSafeSheetName(rawName, usedNames);

                        var maxAns = maxAnsBySheet.TryGetValue(rawName, out var ma) ? ma : 0;
                        var maxOpts = maxOptsBySheet.TryGetValue(rawName, out var mo) ? mo : 0;

                        // Fejléc a fő táblához
                        var header = new List<string> { "Question" };
                        if (sheetName.Equals("Likert", StringComparison.OrdinalIgnoreCase))
                        {
                            for (int i = 0; i < maxAns; i++) header.Add(string.Empty);
                            header.Add("ValueMeanings");
                        }
                        else
                        {
                            for (int i = 0; i < maxAns; i++) header.Add(string.Empty);
                        }

                        // Teljes szélesség: max(main, options)
                        var mainCols = 1 + maxAns + (sheetName.Equals("Likert", StringComparison.OrdinalIgnoreCase) ? 1 : 0);
                        var optionCols = 1 + maxOpts;
                        var totalCols = Math.Max(mainCols, optionCols);
                        while (header.Count < totalCols) header.Add(string.Empty);

                        // Blokkok normalizálása totalCols szélességre
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

                        CreateSheet(
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

        /// <summary>
        /// Létrehoz egy munkalapot és feltölti a megadott fejléccel és adatsorokkal (Main + Opts).
        /// </summary>
        /// <param name="wbPart">A munkafüzet part.</param>
        /// <param name="sheets">A munkafüzet lap-gyűjteménye.</param>
        /// <param name="sheetName">A munkalap neve (Excel kompatibilis, lásd: <see cref="MakeSafeSheetName"/>).</param>
        /// <param name="header">A fő tábla fejléce (első sor).</param>
        /// <param name="blocks">A normalizált blokkok (Main és opcionális Opts).</param>
        /// <param name="explicitSheetId">Opcionális lapazonosító.</param>
        /// <param name="maxAns">A válaszoszlopok maximum száma az adott lapon.</param>
        /// <param name="maxOpts">Az opcióoszlopok maximum száma az adott lapon.</param>
        /// <remarks>
        /// - Fagyasztott felső sor (A2), így a fejléc görgetésnél látható marad.
        /// - A numerikus oszlopok (Likert/SingleChoice/MultipleChoice válaszok) jobbra igazított Number cellák.
        /// - Az "Opts" sor csak akkor íródik ki, ha tartalmaz "Options" címet és legalább 1 opciót.
        /// - Stílusindexek:
        ///   1 = fejléc (félkövér, szürke háttér),
        ///   2 = szöveges adat,
        ///   3 = opciós sor (dőlt, világos háttér),
        ///   4 = numerikus adat (kékes háttér).
        /// </remarks>
        private static void CreateSheet(
            WorkbookPart wbPart, Sheets sheets,
            string sheetName,
            IReadOnlyList<string> header,
            IReadOnlyList<(List<string> Main, List<string> Opts)> blocks,
            uint? explicitSheetId,
            int maxAns,
            int maxOpts)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();

            // Oszlopszélesség-becslés a tartalom alapján
            var cols = BuildAutoColumns(header, blocks, sheetName, maxAns);

            // Fejléc fagyasztás (Pane)
            var views = new SheetViews(new SheetView
            {
                WorkbookViewId = 0,
                Pane = new Pane { VerticalSplit = 1D, TopLeftCell = "A2", ActivePane = PaneValues.BottomLeft, State = PaneStateValues.Frozen }
            });

            wsPart.Worksheet = new Worksheet(views, cols, sheetData);

            // Lap regisztrálása
            var sheet = new Sheet
            {
                Id = wbPart.GetIdOfPart(wsPart),
                SheetId = explicitSheetId ?? (uint)(sheets.Count() + 1),
                Name = sheetName
            };
            sheets.Append(sheet);

            // Fejléc (style 1)
            var headerRow = new Row();
            foreach (var text in header)
                headerRow.Append(TextCell(text, styleIndex: 1));
            sheetData.Append(headerRow);

            // Helyi predikátum: numerikus oszlop-e (a lap típusa és indexe alapján)
            bool IsNumericCol(int colIndex) =>
                sheetName.Equals("Likert", StringComparison.OrdinalIgnoreCase) && colIndex >= 1 && colIndex <= maxAns
                || (sheetName.Equals("SingleChoice", StringComparison.OrdinalIgnoreCase) ||
                    sheetName.Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase)) && colIndex >= 1 && colIndex <= maxAns;

            // Adatsorok kiírása
            foreach (var (Main, Opts) in blocks)
            {
                var dataRow = new Row();
                for (int c = 0; c < Main.Count; c++)
                {
                    if (IsNumericCol(c) &&
                        double.TryParse(Main[c], NumberStyles.Any, CultureInfo.InvariantCulture, out var num))
                        dataRow.Append(NumberCell(num, styleIndex: 4));
                    else
                        dataRow.Append(TextCell(Main[c], styleIndex: 2));
                }
                sheetData.Append(dataRow);

                // Opciós sor (ha van)
                if (Opts is { Count: > 1 })
                {
                    var optRow = new Row();
                    for (int c = 0; c < Opts.Count; c++)
                        optRow.Append(TextCell(Opts[c], styleIndex: 3));
                    sheetData.Append(optRow);
                }
            }
        }

        // ---------------- Helpers: styles & cells ----------------

        /// <summary>
        /// Létrehoz egy szöveges cellát (InlineString) a megadott stílusindexszel.
        /// </summary>
        /// <param name="text">A cella szövege (null esetén üres string).</param>
        /// <param name="styleIndex">A használandó cellaformátum indexe.</param>
        private static Cell TextCell(string? text, uint styleIndex = 0) =>
            new()
            {
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(text ?? string.Empty)),
                StyleIndex = styleIndex
            };

        /// <summary>
        /// Létrehoz egy numerikus cellát (Number) InvariantCulture formázással.
        /// </summary>
        /// <param name="value">A numerikus érték.</param>
        /// <param name="styleIndex">A használandó cellaformátum indexe.</param>
        private static Cell NumberCell(double value, uint styleIndex = 0) =>
            new()
            {
                CellValue = new CellValue(value.ToString(CultureInfo.InvariantCulture)),
                DataType = CellValues.Number,
                StyleIndex = styleIndex
            };

        /// <summary>
        /// Alap stíluslap definiálása: betűk, kitöltések, keretek, cellaformátumok.
        /// </summary>
        /// <remarks>
        /// Fontok: normál, félkövér (fejléc), dőlt (opciók).
        /// Fills: None, Gray125, világos szürkék és kékes kitöltés (numerikus háttér).
        /// Borders: vékony keret.
        /// CellFormats:
        ///  - 0: alap
        ///  - 1: fejléc (félkövér + szürke háttér + középre igazítás)
        ///  - 2: szöveg (keretes, tördelés engedélyezve)
        ///  - 3: opciók (dőlt + világos háttér)
        ///  - 4: szám (kékes háttér, jobbra igazítás)
        /// </remarks>
        private static Stylesheet BuildStylesheet()
        {
            var fonts = new Fonts(
                new Font(),
                new Font(new Bold()),
                new Font(new Italic())
            );

            var fills = new Fills(
                new Fill(new PatternFill { PatternType = PatternValues.None }),
                new Fill(new PatternFill { PatternType = PatternValues.Gray125 }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = "FFD9D9D9" }) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = "FFF2F2F2" }) { PatternType = PatternValues.Solid }),
                new Fill(new PatternFill(new ForegroundColor { Rgb = "FFE6F0FF" }) { PatternType = PatternValues.Solid })
            );

            var borderThin = new Border(
                new LeftBorder { Style = BorderStyleValues.Thin },
                new RightBorder { Style = BorderStyleValues.Thin },
                new TopBorder { Style = BorderStyleValues.Thin },
                new BottomBorder { Style = BorderStyleValues.Thin },
                new DiagonalBorder());

            var borders = new Borders(new Border(), borderThin);

            var cellStyleFormats = new CellStyleFormats(new CellFormat());

            var cellFormats = new CellFormats(
                new CellFormat(), // 0: alap
                new CellFormat // 1: fejléc
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
                new CellFormat // 2: szöveges adat
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
                new CellFormat // 3: opciós sor
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
                new CellFormat // 4: numerikus adat
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

            return new Stylesheet
            {
                Fonts = fonts,
                Fills = fills,
                Borders = borders,
                CellStyleFormats = cellStyleFormats,
                CellFormats = cellFormats
            };
        }

        /// <summary>
        /// Szöveghossz alapján becsült oszlopszélességet ad vissza, szerepkör szerint (kérdés / szám / egyéb).
        /// </summary>
        /// <param name="text">A cella tartalma (többsorost is kezel).</param>
        /// <param name="isQuestionCol">Kérdés oszlop-e (A oszlop).</param>
        /// <param name="isNumericCol">Numerikus oszlop-e (Likert/SC/MC válasz oszlopok).</param>
        /// <returns>Excel szélesség egységben, ésszerű minimum-maximum közé clampelve.</returns>
        private static double EstimateWidth(string? text, bool isQuestionCol, bool isNumericCol)
        {
            var t = text ?? string.Empty;
            var maxLine = t.Split('\n').Max(s => s.Length);
            var baseLen = Math.Max(1, maxLine);

            var pad = isNumericCol ? 1.0 : 2.0;
            var w = baseLen + pad;

            if (isQuestionCol) return Math.Clamp(w, 14.0, 80.0);
            if (isNumericCol) return Math.Clamp(w, 6.0, 30.0);
            return Math.Clamp(w, 8.0, 60.0);
        }

        /// <summary>
        /// Oszlopszélességek felépítése a fejléc és az adatsorok bejárásával.
        /// </summary>
        /// <param name="header">Fejléc sor.</param>
        /// <param name="blocks">Adatblokkok (Main + Opts).</param>
        /// <param name="sheetName">Lapnév (befolyásolja a numerikus oszlop detektálást).</param>
        /// <param name="maxAns">Numerikus válaszoszlopok száma.</param>
        /// <returns><see cref="Columns"/> gyűjtemény egyedi szélességekkel.</returns>
        private static Columns BuildAutoColumns(
            IReadOnlyList<string> header,
            IReadOnlyList<(List<string> Main, List<string> Opts)> blocks,
            string sheetName,
            int maxAns)
        {
            int colCount = header.Count;
            var maxWidths = new double[colCount];

            bool IsNumericCol(int colIndex) =>
                sheetName.Equals("Likert", StringComparison.OrdinalIgnoreCase) && colIndex >= 1 && colIndex <= maxAns
                || (sheetName.Equals("SingleChoice", StringComparison.OrdinalIgnoreCase) ||
                    sheetName.Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase)) && colIndex >= 1 && colIndex <= maxAns;

            // Fejléc szélességek
            for (int c = 0; c < colCount; c++)
                maxWidths[c] = Math.Max(maxWidths[c], EstimateWidth(header[c], c == 0, IsNumericCol(c)));

            // Adatsorok szélességei
            foreach (var (Main, Opts) in blocks)
            {
                for (int c = 0; c < colCount; c++)
                {
                    if (c < Main.Count)
                        maxWidths[c] = Math.Max(maxWidths[c], EstimateWidth(Main[c], c == 0, IsNumericCol(c)));
                    if (c < Opts.Count)
                        maxWidths[c] = Math.Max(maxWidths[c], EstimateWidth(Opts[c], c == 0, false));
                }
            }

            // Columns felépítése (egyenként állított szélességek)
            var cols = new Columns();
            for (uint i = 0; i < colCount; i++)
            {
                cols.Append(new Column { Min = i + 1, Max = i + 1, Width = maxWidths[i], CustomWidth = true });
            }
            return cols;
        }

        /// <summary>
        /// Beállítja a megadott laptípushoz a maximális értéket (válasz/opszió darabszám).
        /// </summary>
        /// <param name="map">Cél szótár.</param>
        /// <param name="sheet">Laptípus kulcs.</param>
        /// <param name="candidate">Új jelölt maximum.</param>
        private static void UpdateMax(Dictionary<string, int> map, string sheet, int candidate)
        {
            if (map.TryGetValue(sheet, out var curr)) map[sheet] = Math.Max(curr, candidate);
            else map[sheet] = candidate;
        }

        /// <summary>
        /// Excel-kompatibilis, egyedi munkalapnév készítése: tiltott karakterek törlése, 31 karakter limit,
        /// ütközés esetén sorszámozás (" (2)", " (3)", ...).
        /// </summary>
        /// <param name="raw">Eredeti lapnév.</param>
        /// <param name="used">Már használt nevek (kis/nagybetű érzéketlen).</param>
        /// <returns>Biztonságos, egyedi lapnév.</returns>
        private static string MakeSafeSheetName(string raw, HashSet<string> used)
        {
            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var name = new string(raw.Where(ch => !invalid.Contains(ch)).ToArray());
            if (string.IsNullOrWhiteSpace(name)) name = "Sheet";
            if (name.Length > 31) name = name[..31];

            var baseName = name;
            int i = 2;
            while (!used.Add(name))
            {
                var suffix = $" ({i++})";
                var baseLen = Math.Min(31 - suffix.Length, baseName.Length);
                name = baseName[..baseLen] + suffix;
            }
            return name;
        }
    }
}
