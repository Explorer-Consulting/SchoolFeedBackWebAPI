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
    /// Administrator szintű, több munkalapos Excel-riportot előállító dokumentumosztály.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="RenderDocument"/> bejárja a <see cref="ReportDocument.ReportComponents"/> listát, 
    /// a komponensek <c>DataSource</c> tulajdonságából (reflexióval) kiolvassa a kérdéstípus-specifikus
    /// adatokat, és laptípusonként normalizált sorokká rendezi.
    /// </para>
    /// <para>
    /// Laptípusok és oszlop-szabályok:
    /// <list type="bullet">
    /// <item><description><c>Likert</c>: <c>Question</c> | <c>Ans1..AnsN</c> | <c>ValueMeanings</c></description></item>
    /// <item><description><c>SingleChoice</c> (REGULAR): <c>Question</c> | <c>Ans1..AnsK</c> | <c>Opt1..OptM</c></description></item>
    /// <item><description><c>SingleChoice+Other</c> (szabad szöveg): <c>Question</c> | <c>Other1..OtherN</c> | <c>Opt1..OptM</c></description></item>
    /// <item><description><c>MultipleChoice</c>: <c>Question</c> | <c>Ans1..AnsK</c> | <c>Opt1..OptM</c></description></item>
    /// <item><description><c>OpenEnded</c>: <c>Question</c> | <c>Ans1..AnsN</c></description></item>
    /// </list>
    /// Minden sor pontosan egy kérdést reprezentál; a sorok lapon belül azonos oszlopszámúra vannak kipárnázva.
    /// </para>
    /// <para>
    /// Megjegyzés: a cellák <see cref="CellValues.InlineString"/> típussal kerülnek kiírásra, ami egyszerű és robusztus.
    /// Nagyon nagy, sok ismétlődő sztringet tartalmazó exportnál megfontolható a SharedStringTable használata.
    /// </para>
    /// </remarks>
    public sealed class AdministratorExcelReportDocument(ReportMetadata metadata, Recipient? recipient = null) : ReportDocument(metadata, recipient)
    {
        /// <summary>
        /// Elkészíti az XLSX fájlt memóriában az aktuális <see cref="ReportDocument.ReportComponents"/> alapján.
        /// </summary>
        /// <returns>Az elkészült Excel fájl bájt tömbje.</returns>
        /// <exception cref="InvalidOperationException">Ha a workbook-part mentése közben hiba történik.</exception>
        public override byte[] RenderDocument()
        {
            using var ms = new MemoryStream();

            using (var doc = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook, true))
            {
                var wbPart = doc.AddWorkbookPart();
                wbPart.Workbook = new Workbook();
                var sheets = wbPart.Workbook.AppendChild(new Sheets());

                // sheet -> sorok (cellalisták)
                var rowsBySheet = new Dictionary<string, List<List<string>>>(StringComparer.OrdinalIgnoreCase);

                // sheet -> max válasz oszlop (Ans*/Other*) száma
                var maxAnsBySheet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                // sheet -> max opció oszlop (Opt*) száma (SingleChoice/MultipleChoice)
                var maxOptsBySheet = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                // Komponensek bejárása és sorokká normalizálása
                foreach (var comp in ReportComponents)
                {
                    // A komponens DataSource tulajdonságának kiolvasása (reflexióval)
                    var ds = comp.GetType().GetProperty("DataSource")?.GetValue(comp);
                    if (ds is null) continue;

                    switch (ds)
                    {
                        case LikertScaleEvaluationData l:
                            {
                                // Question | Ans* | ValueMeanings
                                var row = new List<string> { l.QuestionStatement };
                                row.AddRange(l.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));
                                row.Add(l.ValueMeanings ?? string.Empty);

                                AddRow(rowsBySheet, "Likert", row);
                                UpdateMax(maxAnsBySheet, "Likert", l.Answers.Length);
                                break;
                            }

                        case SingleChoiceEvaluationData s when s.Type == SingleChoice.REGULAR:
                            {
                                // Question | Ans* | Opt*
                                var row = new List<string> { s.QuestionStatement };

                                // Előre definiált opciókhoz tartozó nyers index-válaszok oszloponként (Ans*)
                                row.AddRange(s.QuestionOptionAnswers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                                // Opciók szövege (Opt*)
                                row.AddRange(s.QuestionOptions);

                                AddRow(rowsBySheet, "SingleChoice", row);
                                UpdateMax(maxAnsBySheet, "SingleChoice", s.QuestionOptionAnswers.Length);
                                UpdateMax(maxOptsBySheet, "SingleChoice", s.QuestionOptions.Length);
                                break;
                            }

                        case SingleChoiceEvaluationData s:
                            {
                                // Question | Other* | (opcionálisan) Opt*
                                var row = new List<string> { s.QuestionStatement };

                                // Szabad szöveges válaszok (Other*)
                                row.AddRange(s.QuestionOpenAnswers);

                                // Ha volt előre definiált opciólista, az is kimegy a sor végére (Opt*)
                                if (!s.QuestionOptions.IsDefaultOrEmpty)
                                    row.AddRange(s.QuestionOptions);

                                AddRow(rowsBySheet, "SingleChoice+Other", row);
                                UpdateMax(maxAnsBySheet, "SingleChoice+Other", s.QuestionOpenAnswers.Length);
                                UpdateMax(maxOptsBySheet, "SingleChoice+Other", s.QuestionOptions.Length);
                                break;
                            }

                        // --- MULTIPLE CHOICE 
                        case MultipleChoiceEvaluationData m:
                            {
                                // Question | Ans* | Opt*
                                var row = new List<string> { m.QuestionStatement };

                                // Nyers indexek / kódok (Ans*)
                                row.AddRange(m.Answers.Select(a => a.ToString(CultureInfo.InvariantCulture)));

                                // Opciók szövege (Opt*)
                                row.AddRange(m.AnswerOptions);

                                AddRow(rowsBySheet, "MultipleChoice", row);
                                UpdateMax(maxAnsBySheet, "MultipleChoice", m.Answers.Length);
                                UpdateMax(maxOptsBySheet, "MultipleChoice", m.AnswerOptions.Length);
                                break;
                            }

                        case OpenEndedEvaluationData o:
                            {
                                // Question | Ans*
                                var row = new List<string> { o.QuestionStatement };
                                row.AddRange(o.Answers);

                                AddRow(rowsBySheet, "OpenEnded", row);
                                UpdateMax(maxAnsBySheet, "OpenEnded", o.Answers.Length);
                                break;
                            }
                    }
                }

                // Ha nem érkezett komponens, egy minimális "Empty" lap készül
                if (rowsBySheet.Count == 0)
                {
                    CreateSheet(wbPart, sheets, "Empty",
                        header: new[] { "Question" },
                        rows: new List<List<string>> { new() { "—" } });
                }
                else
                {
                    uint sheetId = 1;
                    var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    // Laponként fejléc és egységes oszlopszám kialakítása, majd lap létrehozása
                    foreach (var (rawName, rows) in rowsBySheet)
                    {
                        var sheetName = MakeSafeSheetName(rawName, usedNames);

                        // Fejléc inicializálás
                        var header = new List<string> { "Question" };
                        var maxAns = maxAnsBySheet.TryGetValue(rawName, out var ma) ? ma : 0;
                        var maxOpts = maxOptsBySheet.TryGetValue(rawName, out var mo) ? mo : 0;

                        if (sheetName.Equals("Likert", StringComparison.OrdinalIgnoreCase))
                        {
                            for (int i = 1; i <= maxAns; i++) header.Add("Ans" + i);
                            header.Add("ValueMeanings");
                            // Likert sorok hossza = 1 (Question) + Ans* + 1 (ValueMeanings)
                            PadRowsToWidth(rows, 1 + maxAns + 1);
                        }
                        else if (sheetName.Equals("SingleChoice", StringComparison.OrdinalIgnoreCase) ||
                                 sheetName.Equals("MultipleChoice", StringComparison.OrdinalIgnoreCase))
                        {
                            for (int i = 1; i <= maxAns; i++) header.Add("Ans" + i);
                            for (int i = 1; i <= maxOpts; i++) header.Add("Opt" + i);
                            // Question + Ans* + Opt*
                            PadRowsToWidth(rows, 1 + maxAns + maxOpts);
                        }
                        else if (sheetName.Equals("SingleChoice+Other", StringComparison.OrdinalIgnoreCase))
                        {
                            for (int i = 1; i <= maxAns; i++) header.Add("Other" + i);
                            for (int i = 1; i <= maxOpts; i++) header.Add("Opt" + i);
                            // Question + Other* + (opcionális) Opt*
                            PadRowsToWidth(rows, 1 + maxAns + maxOpts);
                        }
                        else // OpenEnded, stb.
                        {
                            for (int i = 1; i <= maxAns; i++) header.Add("Ans" + i);
                            PadRowsToWidth(rows, 1 + maxAns);
                        }

                        CreateSheet(wbPart, sheets, sheetName, header, rows, sheetId++);
                    }
                }

                wbPart.Workbook.Save();
            }

            Data = ms.ToArray();
            return Data;
        }

        /// <summary>
        /// Hozzáad egy új sort a megadott munkalaphoz. A null értékeket üres sztringgé normalizálja.
        /// </summary>
        /// <param name="map">Lapnév → sorok (cellalista) gyűjtemény.</param>
        /// <param name="sheet">A munkalap neve (logikai azonosító).</param>
        /// <param name="values">A sorban szereplő cellaértékek.</param>
        private static void AddRow(Dictionary<string, List<List<string>>> map, string sheet, IEnumerable<string> values)
        {
            if (!map.TryGetValue(sheet, out var list))
                map[sheet] = list = new List<List<string>>();
            list.Add(values.Select(v => v ?? string.Empty).ToList());
        }

        /// <summary>
        /// Frissíti a lapon belüli maximális oszlopszámot (válasz- vagy opcióoszlopok esetén).
        /// </summary>
        /// <param name="map">Lapnév → maximális oszlopszám táblázat.</param>
        /// <param name="sheet">A munkalap neve.</param>
        /// <param name="candidate">Jelölt érték, amellyel a jelenlegi maximumot összevetjük.</param>
        private static void UpdateMax(Dictionary<string, int> map, string sheet, int candidate)
        {
            if (map.TryGetValue(sheet, out var curr)) map[sheet] = Math.Max(curr, candidate);
            else map[sheet] = candidate;
        }

        /// <summary>
        /// Kipárnázza a sorokat üres sztringekkel, hogy minden sor azonos számú oszlopot tartalmazzon.
        /// </summary>
        /// <param name="rows">A lap sorai (cellalisták).</param>
        /// <param name="totalCols">A kívánt oszlopszám.</param>
        private static void PadRowsToWidth(List<List<string>> rows, int totalCols)
        {
            foreach (var r in rows)
                while (r.Count < totalCols) r.Add(string.Empty);
        }

        /// <summary>
        /// Új munkalapot hoz létre és feltölti a megadott fejléccel és adatsorokkal.
        /// </summary>
        /// <param name="wbPart">A workbook-part, amelyhez a munkalap csatlakozni fog.</param>
        /// <param name="sheets">A workbookhoz tartozó <see cref="Sheets"/> gyűjtemény.</param>
        /// <param name="sheetName">A munkalap megjelenített neve (Excel szabályoknak megfelelően).</param>
        /// <param name="header">Fejléc oszlopnevek sorrendben.</param>
        /// <param name="rows">Adatsorok (minden belső lista egy sor).</param>
        /// <param name="explicitSheetId">Opcionális egyedi SheetId; ha nincs megadva, automatikusan generálódik.</param>
        private static void CreateSheet(
            WorkbookPart wbPart, Sheets sheets,
            string sheetName,
            IReadOnlyList<string> header,
            IReadOnlyList<List<string>> rows,
            uint? explicitSheetId = null)
        {
            var wsPart = wbPart.AddNewPart<WorksheetPart>();
            var sheetData = new SheetData();
            wsPart.Worksheet = new Worksheet(sheetData);

            var sheet = new Sheet
            {
                Id = wbPart.GetIdOfPart(wsPart),
                SheetId = explicitSheetId ?? (uint)(sheets.Count() + 1),
                Name = sheetName
            };
            sheets.Append(sheet);

            uint r = 1;

            // Fejléc sor
            var headerRow = new Row { RowIndex = r++ };
            for (int c = 0; c < header.Count; c++)
                headerRow.Append(TextCell(ColumnName(c) + headerRow.RowIndex, header[c]));
            sheetData.Append(headerRow);

            // Adatsorok
            foreach (var data in rows)
            {
                var row = new Row { RowIndex = r++ };
                for (int c = 0; c < data.Count; c++)
                    row.Append(TextCell(ColumnName(c) + row.RowIndex, data[c]));
                sheetData.Append(row);
            }
        }

        /// <summary>
        /// Szöveges (InlineString) cellát hoz létre megadott címen.
        /// </summary>
        /// <param name="address">Cella hivatkozás (pl. <c>A1</c>, <c>BC12</c>).</param>
        /// <param name="text">A cellába írandó szöveg (null esetén üres).</param>
        /// <returns>Felépített <see cref="Cell"/> objektum.</returns>
        private static Cell TextCell(string address, string? text) =>
            new Cell
            {
                CellReference = address,
                DataType = CellValues.InlineString,
                InlineString = new InlineString(new Text(text ?? string.Empty))
            };

        /// <summary>
        /// 0-bázisú oszlopindexből Excel oszlopnevet generál (A..Z, AA..AZ, stb.).
        /// </summary>
        /// <param name="index">0-bázisú oszlopindex.</param>
        /// <returns>Az Excel oszlopnév.</returns>
        private static string ColumnName(int index)
        {
            index += 1;
            var stack = new Stack<char>();
            while (index > 0)
            {
                index--;
                stack.Push((char)('A' + (index % 26)));
                index /= 26;
            }
            return new string(stack.ToArray());
        }

        /// <summary>
        /// Excel-kompatibilis, egyedi lapnevet készít.
        /// </summary>
        /// <param name="raw">Bejövő nyers név.</param>
        /// <param name="used">Már felhasznált nevek halmaza (ütközés esetén sorszámoz).</param>
        /// <returns>Valid, max. 31 karakteres, tiltott jelektől mentes és egyedi lapnév.</returns>
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
