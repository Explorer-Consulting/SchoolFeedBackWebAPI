using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FeedBackApp.Core.Model
{
    public static class AnswerFormats
    {
        public const string DateFormat = "yyyy-MM-dd";

        public static bool TryParseNumber(string? s, out double value) =>
            double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            && double.IsFinite(value);

        public static bool TryParseDate(string? s, out DateOnly value) =>
            DateOnly.TryParseExact(s, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out value);
    }
}
