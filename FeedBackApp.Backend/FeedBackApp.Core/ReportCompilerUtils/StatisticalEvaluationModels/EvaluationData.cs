using FeedBackApp.Core.ReportCompilerUtils.ReportComponents;

namespace FeedBackApp.Core.ReportCompilerUtils.StatisticalEvaluationModels
{
    public abstract class EvaluationData
    {
        public EvaluationData() { }
        protected abstract ReportComponent EvaluateData();
        protected virtual double CalculateMeanValue(in List<int> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            double mean =  data.Average();
            return mean;
        }
        protected virtual double CalculateMedianValue(List<int> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            data.Sort();
            int dataCount = data.Count;
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
        protected virtual int CalculateModeValue(List<int> data)
        {
            ArgumentNullException.ThrowIfNull(data);

            int mode = data[0];
            int maxCount = 1;

            int current = data[0];
            int count = 1;

            for (int i = 1; i < data.Count; i++)
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
        protected virtual double CalculateStandardDeviation(in List<int> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            int dataCount = data.Count;
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
        protected virtual int GetMaximumValue(in List<int> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return data.Max();
        }
        protected virtual int GetMinimumValue(in List<int> data)
        {
            ArgumentNullException.ThrowIfNull(data);
            return data.Min();
        }
        protected virtual double CalculateAgreementRate(in List<int> data, in int positiveThreshold)
        {
            ArgumentNullException.ThrowIfNull(data);
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
        protected virtual double CalculateSatisfactionIndex(in List<int> data, in int minScale, in int maxScale)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentOutOfRangeException.ThrowIfLessThan(minScale, 0);
            ArgumentOutOfRangeException.ThrowIfLessThan(maxScale, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(minScale, maxScale);

            double mean = data.Average();
            double satisfactionIndex = ((mean - minScale) / (maxScale - minScale)) * 100.0;
            return satisfactionIndex;
        }
    }
}
