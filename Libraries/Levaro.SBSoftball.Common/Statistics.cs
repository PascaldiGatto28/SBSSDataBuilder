namespace Levaro.SBSoftball.Common
{
    /// <summary>
    /// TODO: Complete this or move to another class
    /// </summary>
    public class Statistics
    {
        public Statistics()
        {
            Title = string.Empty;
        }

        public string Title
        {
            get;
            set;
        }

        public double Minimum
        {
            get;
            set;
        }

        public double Maximum
        {
            get;
            set;
        }

        public double Mean
        {
            get;
            set;
        }

        public double Variance
        {
            get;
            set;
        }

        public double StdDev
        {
            get;
            set;
        }

        public int Count
        {
            get;
            set;
        }

        public static Statistics GetStatistics(IEnumerable<double> source, string? title = null, double? mean = null)
        {
            double sum = 0.0;
            double sumOfSquares = 0.0;
            int count = source.Count();
            double n = (double)count;
            string description = title ?? $"Statistics for {count:#,###} items";

            double average;
            double variance;
            if (mean == null)
            {
                foreach (double value in source)
                {
                    sum += value;
                    sumOfSquares += (value * value);
                }

                average = sum / n;
                variance = (sumOfSquares / n) - (average * average);
            }
            else
            {
                foreach (double value in source)
                {
                    double difference = mean.Value - value;
                    sumOfSquares += (difference * difference);
                }

                average = mean.Value;
                variance = sumOfSquares / n;
            }

            double stdDev = Math.Sqrt(variance);
            double min = source.ToList().Min();
            double max = source.ToList().Max();

            return new Statistics()
            {
                Title = description,
                Minimum = Math.Round(min, 3),
                Maximum = Math.Round(max, 3),
                Mean = Math.Round(average, 3),
                Variance = Math.Round(variance, 3),
                StdDev = Math.Round(stdDev, 3),
                Count = count
                //SourceData = source
            };
        }

        public static Statistics GetStatistics(IEnumerable<int> source, string title = null, double? mean = null)
        {
            IEnumerable<double> values = new List<double>();
            if (source != null)
            {
                values = source.Cast<double>().ToList();
            }

            return GetStatistics(values, title, mean);
        }
    }
}
