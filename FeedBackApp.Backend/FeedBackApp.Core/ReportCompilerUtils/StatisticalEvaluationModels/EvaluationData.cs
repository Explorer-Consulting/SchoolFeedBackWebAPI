using QuestPDF.Infrastructure;
using System.Collections.Immutable;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    /// <summary>
    /// Statisztikai kiértékelések absztrakt alaposztálya.
    /// <para>
    /// Feladata: a beérkező nyers adatok (tipikusan egész értékek) feldolgozása,
    /// leíró statisztikák kiszámítása és a megfelelő riportkomponens összeállítása.
    /// </para>
    /// </summary>
    public abstract class EvaluationData
    {
        /// <summary>Alap konstruktor.</summary>
        public EvaluationData() { }

        /// <summary>
        /// A nyers adatok kiértékelése és a kiszámított mutatók (mezők/property-k) feltöltése.
        /// </summary>
        public abstract EvaluationData EvaluateData();

        /// <summary>
        /// A hozzá tartozó QuestPDF komponens legyártása (megjelenítéshez).
        /// </summary>
        public abstract IComponent CompileComponent();

        /// <summary>
        /// Átlag számítása. Üres/default tömb esetén kivételt dob.
        /// </summary>
        protected virtual double CalculateMeanValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateMeanValue() is empty or in default state");
            return data.Average();
        }

        /// <summary>
        /// Medián számítása (statisztikai közép). A bemenetet nem módosítja.
        /// </summary>
        protected virtual double CalculateMedianValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from CalculateMedianValue() is empty or in default state");

            // Rendezett másolat készítése
            var arr = data.ToArray();
            Array.Sort(arr);
            int n = arr.Length;

            if ((n & 1) == 1) // páratlan
                return arr[n / 2];

            // páros
            int a = arr[(n / 2) - 1];
            int b = arr[n / 2];
            return (a + b) / 2.0;
        }

        /// <summary>
        /// Módusz (leggyakoribb érték) meghatározása, rendezést nem igényel.
        /// Döntetlen esetén a legkisebb értéket adja vissza.
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
        /// Szórás számítása Welford-féle online algoritmussal.
        /// <para>Megjegyzés: ez <b>populációs</b> szórás (N nevező). Mintaszóráshoz (N-1) módosítsd a nevezőt.</para>
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

            double variance = m2 / count; // populációs
            return Math.Sqrt(variance);
        }

        /// <summary>Maximum érték meghatározása.</summary>
        protected virtual int GetMaximumValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from GetMaximumValue() is empty or in default state");
            return data.Max();
        }

        /// <summary>Minimum érték meghatározása.</summary>
        protected virtual int GetMinimumValue(ImmutableArray<int> data)
        {
            if (data.IsDefaultOrEmpty)
                throw new ArgumentException("ImmutableArray<int> from GetMinimumValue() is empty or in default state");
            return data.Min();
        }

        /// <summary>
        /// Egyetértési arány (pozitívnak tekintett válaszok aránya, %).
        /// <para><b>Definíció:</b> x &gt; <paramref name="positiveThreshold"/> esetén pozitív.</para>
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
        /// Elégedettségi index (0–100%), az átlag skálán történő normalizálásával.
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
        /// Abszolút gyakoriságok számítása (opció → darab).
        /// </summary>
        protected virtual Dictionary<string, int> CalculateFrequency(
    ImmutableArray<string> answerOptions,
    ImmutableArray<int> data)
        {
            if (answerOptions.IsDefaultOrEmpty)
                throw new ArgumentException("answerOptions is empty/default");

            // Ha nincs adat, adj vissza 0-kat inkább kivétel helyett:
            var frequencies = new Dictionary<string, int>(answerOptions.Length);
            foreach (var option in answerOptions) frequencies[option] = 0;
            if (data.IsDefaultOrEmpty) return frequencies;

            foreach (var raw in data)
            {
                var idx = raw;

                // 1-alapú → 0-alapú normalizáció (toleráns)
                if (idx >= 1 && idx <= answerOptions.Length) idx--;

                if ((uint)idx < (uint)answerOptions.Length)
                {
                    frequencies[answerOptions[idx]]++;
                }
                else
                {
                    // invalid index → kihagyjuk (opcionálisan: log)
                    // TODO: log warn with question id/context
                }
            }

            return frequencies;
        }

        /// <summary>
        /// Relatív gyakoriságok kiszámítása %-ban az abszolút gyakoriságokból.
        /// Ha <paramref name="totalSelections"/> nincs megadva, az abszolút gyakoriságok összege az alap.
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
        /// Rangsor csökkenő abszolút gyakoriság szerint, majd név szerint (stabil megjelenítéshez).
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
        /// Domináns opció (legmagasabb gyakoriság). Üres bemenet esetén <c>null</c>.
        /// </summary>
        protected virtual KeyValuePair<string, int>? GetDominantOption(
            IReadOnlyDictionary<string, int> absoluteFrequencies)
        {
            ArgumentNullException.ThrowIfNull(absoluteFrequencies);
            if (absoluteFrequencies.Count == 0) return null;
            return absoluteFrequencies.MaxBy(kv => kv.Value);
        }

        /// <summary>
        /// Módusz robusztus meghatározása (rendezetlenség mellett is).
        /// Döntetlen esetén a legkisebb értéket adja vissza.
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
