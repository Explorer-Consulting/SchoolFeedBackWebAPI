using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public abstract class EvaluationData
    {
        public EvaluationData() { }
        public abstract EvaluationData EvaluateData();
        public abstract IComponent CompileComponent();
        protected virtual double CalculateMeanValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from CalculateMeanValue() is empty or in default state");
            double mean =  data.Average();
            return mean;
        }
        protected virtual double CalculateMedianValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from CalculateMedianValue() is empty or in default state");
            data.Sort();
            int dataCount = data.Length;
            if (dataCount % 2 == 1)
            {
                return data[dataCount / 2];
            }
            else
            {
                int a = data[(dataCount / 2) - 1];
                int b = data[dataCount / 2];
                return ((a + b) / 2.0);
            }
        }
        protected virtual int CalculateModeValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from CalculateModeValue() is empty or in default state");

            int mode = data[0];
            int maxCount = 1;

            int current = data[0];
            int count = 1;

            for (int i = 1; i < data.Length; i++)
            {
                if (data[i] == current)
                {
                    count++;
                }
                else
                {
                    if (count > maxCount)
                    {
                        maxCount = count;
                        mode = current;
                    }

                    current = data[i];
                    count = 1;
                }
            }

            if (count > maxCount)
            {
                mode = current;
            }

            return mode;
        }
        protected virtual double CalculateStandardDeviation(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from StandardDeviation() is empty or in default state");
            double mean = data.Average();
            double m = 0.0;
            int count = 0;
            foreach(var x in data)
            {
                count++;
                double delta1 = x - mean;
                mean += delta1 / count;
                double delta2 = x - mean;
                m += delta1 * delta2;
            }
            if (count < 2)
                return 0.0;
            double variance = m / count;
            return Math.Sqrt(variance);
        }
        protected virtual int GetMaximumValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from GetMaximumValue() is empty or in default state");
            return data.Max();
        }
        protected virtual int GetMinimumValue(ImmutableArray<int> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return data.Min();
        }
        protected virtual double CalculateAgreementRate(ImmutableArray<int> data, in int positiveThreshold)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from CalculateAgreementRate() is empty or in default state");
            int total = 0;
            int positiveCount = 0;
            foreach(var x in data)
            {
                total++;
                if (x > positiveThreshold)
                    positiveCount++;
            }
            return (double)positiveCount / total * 100;
        }
        protected virtual double CalculateSatisfactionIndex(ImmutableArray<int> data, in int minScale, in int maxScale)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from CalculateSatisfactionIndex is empty or in default state");
            ArgumentOutOfRangeException.ThrowIfLessThan(minScale, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxScale, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(minScale, maxScale);

            double mean = data.Average();
            double satisfactionIndex = ((mean - minScale) / (maxScale - minScale)) * 100.0;
            return satisfactionIndex;
        }
        protected virtual Dictionary<string, int> CalculateFrequency(
            ImmutableArray<string> answerOptions,
            ImmutableArray<int> data
        )
        {
            if (answerOptions.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<string> from CalculateFrequency() is empty or in default state");
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from CalculateFrequency() is empty or in default state");

            // induló értékek
            var frequencies = new Dictionary<string, int>(answerOptions.Length);
            foreach (var option in answerOptions)
                frequencies[option] = 0;

            foreach (var answerIndex in data)
            {
                if (answerIndex < 0 || answerIndex >= answerOptions.Length)
                    throw new ArgumentOutOfRangeException(
                        nameof(data),
                        $"Érvénytelen opció index: {answerIndex}. Engedélyezett: 0..{answerOptions.Length - 1}");

                var key = answerOptions[answerIndex];
                frequencies[key]++;
            }

            return frequencies;
        }

        /// <summary>
        /// Relatív gyakoriság (százalék) az abszolút gyakoriságokból.
        /// Ha totalSelections nincs megadva, az összesített darabszámot használja.
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
        /// Rangsor: csökkenő sorrend abszolút gyakoriság szerint (majd név szerint).
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
        /// Domináns opció (legnagyobb gyakoriság). Ha üres, nullt ad.
        /// </summary>
        protected virtual KeyValuePair<string, int>? GetDominantOption(
            IReadOnlyDictionary<string, int> absoluteFrequencies)
        {
            ArgumentNullException.ThrowIfNull(absoluteFrequencies);
            if (absoluteFrequencies.Count == 0) return null;
            return absoluteFrequencies.MaxBy(kv => kv.Value);
        }

        /// <summary>
        /// Módusz robusztus meghatározása (nem igényel előre rendezést).
        /// Döntetlen esetén a legkisebb értéket adja vissza.
        /// </summary>
        protected virtual int CalculateModeValueRobust(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty) throw new ArgumentException("ImmutableArray<int> from CalculateModeValueRobust is empty or in default state");
            if (data.Length == 0) throw new ArgumentException("Üres lista.", nameof(data));

            var counts = new Dictionary<int, int>();
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
