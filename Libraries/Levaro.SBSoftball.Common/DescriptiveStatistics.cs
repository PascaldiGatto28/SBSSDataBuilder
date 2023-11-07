namespace Levaro.SBSoftball.Common
{
    /// <summary>
    /// Encapsulates the descriptive statistics for a sequence of data values. 
    /// </summary>
    /// <remarks>
    /// Instances are created via the static method
    /// <see cref="GetStatistics(IEnumerable{double}, string?, double?)"/>. The properties cannot be set 
    /// except via the static method and deserialization.
    /// </remarks>
    public class DescriptiveStatistics
    {
        /// <summary>
        /// Creates a new instance of the class. Although the static methods are used to programatically create objects, this
        /// constructor is required to initialized the non-nullable properties and to set the <see cref="IsEmpty"/> property
        /// to <c>true</c>.
        /// </summary>
        private DescriptiveStatistics()
        {
            Title = string.Empty;
            OrderedSequence = Enumerable.Empty<double>();
            IsEmpty = true;
        }

        /// <summary>
        /// If the value is true, the object should not be viewed as successfully instantiated. It is initialized 
        /// to <c>true</c> by the default constructor and set to <c>false</c> if 
        /// the static method <see cref="GetStatistics(IEnumerable{double}, string?, double?)"/> successfully 
        /// returns an instance.
        /// </summary>
        public bool IsEmpty
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes an optional title of the instance of the class. If the title is not specified in the
        /// static method <see cref="GetStatistics(IEnumerable{double}, string?, double?)"/>, the title
        /// <code language="cs">"Statistics for {count:#,###} items"</code>, where <c>count</c> is the number of values
        /// in the sequence used to compute the descriptive statistics.
        /// </summary>
        public string Title
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the minimum value of the sequence of items.
        /// </summary>
        public double Minimum
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the maximum value of the sequence of items.
        /// </summary>
        public double Maximum
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the mean (average) of the sequence of items.
        /// </summary>
        public double Mean
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the median of the sequence of items.
        /// </summary>
        public double Median
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the variance of the sequence of items.
        /// </summary>
        public double Variance
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the standard deviation of the sequence of items.
        /// </summary>
        public double StdDev
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the number of items in the sequence.
        /// </summary>
        public int Count
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the sequence of items in ascending order.
        /// </summary>
        public IEnumerable<double> OrderedSequence
        {
            get;
            init;
        }

        /// <summary>
        /// Constructs a <c>DescriptiveStatistics</c> object using the specified sequence of <c>double</c> items.
        /// </summary>
        /// <param name="source">The sequence of items of type <c>double</c></param>
        /// <param name="title">An optional title. The default is just "Statistics for [count] items" where [count] is
        /// the number of items in the <paramref name="source"/> sequence.</param>
        /// <param name="mean">
        /// If <c>null</c> (which is the default), the value is calculated from the <c>source</c> sequence. Otherwise it is used
        /// to calculate the variance and sums of squares. Generally this should not be set unless the data has been
        /// normalized.
        /// </param>
        /// <returns>A <c>DescriptiveStatistics</c> instance. If the <paramref name="source"/> is <c>null</c> or of length
        /// zero, the empty instance (<c>IsEmpty</c> is <c>true</c>) is returned.</returns>
        public static DescriptiveStatistics GetStatistics(IEnumerable<double> source, string? title = null, double? mean = null)
        {
            DescriptiveStatistics stats = new();
            if ((source != null) && source.Any())
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
                double median = 0.0;

                List<double> orderedList = source.ToList().OrderBy(x => x).ToList();
                if (count % 2 == 0)
                {
                    median = orderedList.Skip((count / 2) - 1).Take(2).Average();
                }
                else
                {
                    median = orderedList[count / 2];
                }

                stats = new DescriptiveStatistics()
                {
                    IsEmpty = false,
                    Title = description,
                    Minimum = Math.Round(min, 3),
                    Maximum = Math.Round(max, 3),
                    Mean = Math.Round(average, 3),
                    Median = Math.Round(median, 3),
                    Variance = Math.Round(variance, 3),
                    StdDev = Math.Round(stdDev, 3),
                    Count = count,
                    OrderedSequence = orderedList.Select(e => Math.Round(e, 3))
                };
            }

            return stats;
        }
    }
}
