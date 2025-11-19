using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
{
    internal class NameUtils
    {
        /// <summary>
        /// Normalizes a name by removing invalid characters, trimming to a max length,
        /// and ensuring uniqueness inside the provided name set.
        /// </summary>
        /// <param name="raw">Original name to normalize.</param>
        /// <param name="usedNames">Set of already used names (checked for uniqueness).</param>
        /// <param name="invalidChars">Characters to remove from the input name.</param>
        /// <param name="maxLength">Maximum allowed length after trimming.</param>
        /// <param name="defaultName">Fallback name if the input becomes empty.</param>
        /// <returns>Safe, unique sheet name.</returns>
        /// </summary>
        public static string MakeUniqueName(
            string raw,
            ISet<string> usedNames,
            IEnumerable<char> invalidChars,
            int maxLength = 31,
            string defaultName = "Sheet")
        {
            // if raw is null or whitespace, use default
            if (string.IsNullOrWhiteSpace(raw)) raw = defaultName;

            var name = new string(raw.Where(ch => !invalidChars.Contains(ch)).ToArray());

            // if name is empty after removing invalid chars, use default
            if (string.IsNullOrWhiteSpace(name)) name = defaultName;

            // if name exceeds max length, trim it
            if (name.Length > maxLength)
                name = name[..maxLength];

            var uniqueName = name;
            int counter = 2;

            // ensure uniqueness
            while (!usedNames.Add(name))
            {
                string suffix = $" ({counter})";
                int allowedLength = Math.Min(maxLength - suffix.Length, uniqueName.Length);
                name = uniqueName[..allowedLength] + suffix;
            }

            return name;
        }
    }
}
