using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Abstract base class for statistical evaluation models.
    /// <para>
    /// Its responsibility is to process raw input data (typically integer values),
    /// calculate descriptive statistics, and assemble the corresponding report component.
    /// </para>
    /// </summary>
    public abstract class EvaluationData
    {
        /// <summary>Default constructor.</summary>
        public EvaluationData() { }

        /// <summary>
        /// Evaluates the raw data and populates the calculated indicators (fields/properties).
        /// </summary>
        public abstract EvaluationData EvaluateData();

        /// <summary>
        /// Creates the corresponding QuestPDF component (for rendering purposes).
        /// </summary>
        public abstract IComponent CompileComponent();

        /// <summary>
        /// Calculates the mean value.  
        /// Throws an exception if the array is empty or in default state.
        /// </summary>
        protected virtual double CalculateMeanValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateMeanValue() is empty or in default state");
            return data.Average();
        }

        /// <summary>
        /// Calculates the median (statistical middle).  
        /// Does not modify the input array.
        /// </summary>
        protected virtual double CalculateMedianValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateMedianValue() is empty or in default state");

            var arr = data.ToArray();
            Array.Sort(arr);
            int n = arr.Length;

            if ((n & 1) == 1) // odd length
                return arr[n / 2];

            // even length
            int a = arr[(n / 2) - 1];
            int b = arr[n / 2];
            return (a + b) / 2.0;
        }

        /// <summary>
        /// Calculates the mode (most frequent value) without requiring sorting.  
        /// In case of ties, the smallest value is returned.
        /// </summary>
        protected virtual int CalculateModeValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateModeValue() is empty or in default state");

            var counts = new Dictionary<int, int>(capacity: Math.Max(4, data.Length));
            foreach (var x in data)
            {
                counts.TryGetValue(x, out int c);
                counts[x] = c + 1;
            }

            int bestVal = default;
            int bestCount = -1;
            foreach (var kv in counts)
            {
                if (kv.Value > bestCount || (kv.Value == bestCount && kv.Key < bestVal))
                {
                    bestVal = kv.Key;
                    bestCount = kv.Value;
                }
            }
            return bestVal;
        }

        /// <summary>
        /// Calculates the standard deviation using Welford’s online algorithm.  
        /// <para>Note: this is a <b>population</b> standard deviation (denominator N).  
        /// For sample standard deviation, adjust the denominator to (N-1).</para>
        /// </summary>
        protected virtual double CalculateStandardDeviation(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from StandardDeviation() is empty or in default state");

            double mean = 0.0;
            double m2 = 0.0;
            int count = 0;

            foreach (var x in data)
            {
                count++;
                double delta = x - mean;
                mean += delta / count;
                double delta2 = x - mean;
                m2 += delta * delta2;
            }

            if (count < 2)
                return 0.0;

            double variance = m2 / count; // population
            return Math.Sqrt(variance);
        }

        /// <summary>Returns the maximum value.</summary>
        protected virtual int GetMaximumValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from GetMaximumValue() is empty or in default state");
            return data.Max();
        }

        /// <summary>Returns the minimum value.</summary>
        protected virtual int GetMinimumValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from GetMinimumValue() is empty or in default state");
            return data.Min();
        }

        /// <summary>
        /// Calculates the agreement rate (percentage of responses considered positive).  
        /// <para><b>Definition:</b> a value is considered positive if <c>x &gt; <paramref name="positiveThreshold"/></c>.</para>
        /// </summary>
        protected virtual double CalculateAgreementRate(ImmutableArray<int> data, in int positiveThreshold)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateAgreementRate() is empty or in default state");

            int total = 0;
            int positive = 0;
            foreach (var x in data)
            {
                total++;
                if (x > positiveThreshold) positive++;
            }
            return total == 0 ? 0.0 : (double)positive / total * 100.0;
        }

        /// <summary>
        /// Calculates the satisfaction index (0–100%), normalized on the scale range.
        /// </summary>
        protected virtual double CalculateSatisfactionIndex(ImmutableArray<int> data, in int minScale, in int maxScale)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateSatisfactionIndex is empty or in default state");
            ArgumentOutOfRangeException.ThrowIfLessThan(minScale, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxScale, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(minScale, maxScale);

            double mean = data.Average();
            return ((mean - minScale) / (maxScale - minScale)) * 100.0;
        }

        /// <summary>
        /// Calculates absolute frequencies (option → count).
        /// </summary>
        protected virtual Dictionary<string, int> CalculateFrequency(
            ImmutableArray<string> answerOptions,
            ImmutableArray<int> data)
        {
            if (answerOptions.IsDefaultOrEmpty)
                throw new ArgumentException("answerOptions is empty/default");

            // If no data, return zeros instead of throwing:
            var frequencies = new Dictionary<string, int>(answerOptions.Length);
            foreach (var option in answerOptions) frequencies[option] = 0;
            if (data.IsDefaultOrEmpty) return frequencies;

            foreach (var raw in data)
            {
                var idx = raw;

                // Normalize from 1-based → 0-based (tolerant)
                if (idx >= 1 && idx <= answerOptions.Length) idx--;

                if ((uint)idx < (uint)answerOptions.Length)
                {
                    frequencies[answerOptions[idx]]++;
                }
                else
                {
                    // invalid index → skipped (optionally: log)
                    // TODO: log warning with question id/context
                }
            }

            return frequencies;
        }

        /// <summary>
        /// Calculates relative frequencies (%) from absolute frequencies.  
        /// If <paramref name="totalSelections"/> is not provided, the sum of absolute frequencies is used as the denominator.
        /// </summary>
        protected virtual Dictionary<string, double> CalculateRelativeFrequencyPercent(
            IReadOnlyDictionary<string, int> absoluteFrequencies,
            int? totalSelections = null)
        {
            ArgumentNullException.ThrowIfNull(absoluteFrequencies);
            int total = totalSelections ?? absoluteFrequencies.Values.Sum();
            if (total == 0)
                return absoluteFrequencies.Keys.ToDictionary(k => k, _ => 0d);

            var result = new Dictionary<string, double>(absoluteFrequencies.Count);
            foreach (var kv in absoluteFrequencies)
                result[kv.Key] = (double)kv.Value / total * 100.0;

            return result;
        }

        /// <summary>
        /// Ranks options by descending absolute frequency, then by name (for stable output).
        /// </summary>
        protected virtual List<KeyValuePair<string, int>> RankByFrequency(
            IReadOnlyDictionary<string, int> absoluteFrequencies)
        {
            ArgumentNullException.ThrowIfNull(absoluteFrequencies);
            return absoluteFrequencies
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key, StringComparer.CurrentCultureIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Returns the dominant option (highest frequency).  
        /// Returns <c>null</c> if the input is empty.
        /// </summary>
        protected virtual KeyValuePair<string, int>? GetDominantOption(
            IReadOnlyDictionary<string, int> absoluteFrequencies)
        {
            ArgumentNullException.ThrowIfNull(absoluteFrequencies);
            if (absoluteFrequencies.Count == 0) return null;
            return absoluteFrequencies.MaxBy(kv => kv.Value);
        }

        /// <summary>
        /// Calculates the mode in a robust way (handles unsorted input).  
        /// In case of ties, the smallest value is returned.
        /// </summary>
        protected virtual int CalculateModeValueRobust(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateModeValueRobust is empty or in default state");

            var counts = new Dictionary<int, int>(capacity: Math.Max(4, data.Length));
            foreach (var x in data)
            {
                counts.TryGetValue(x, out int c);
                counts[x] = c + 1;
            }

            int bestVal = default;
            int bestCount = -1;
            foreach (var kv in counts)
            {
                if (kv.Value > bestCount || (kv.Value == bestCount && kv.Key < bestVal))
                {
                    bestVal = kv.Key;
                    bestCount = kv.Value;
                }
            }
            return bestVal;
        }
    }
}
