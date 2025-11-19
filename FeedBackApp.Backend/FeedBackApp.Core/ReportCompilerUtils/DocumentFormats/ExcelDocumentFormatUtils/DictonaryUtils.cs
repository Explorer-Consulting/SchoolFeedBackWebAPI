using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedBackApp.Core.ReportCompilerUtils.DocumentFormats.ExcelDocumentFormatUtils
{

    /// <summary>
    // Helper for updating max values in dictionary
    /// </summary>
    internal static class DictonaryUtils
    {
        /// <summary>
        /// Updates the stored maximum value for the given key.
        /// If the key does not exist yet, it is added with the candidate value.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type.</typeparam>
        /// <param name="map">Target dictionary.</param>
        /// <param name="key">Key to update.</param>
        /// <param name="value">New candidate value.</param>
       
        internal static void UpdateMax<TKey>(this IDictionary<TKey, int> map, TKey key, int value)
        {
            if (map.TryGetValue(key, out var curr))
                map[key] = Math.Max(curr, value);
            else
                map[key] = value;
        }
    }
}
