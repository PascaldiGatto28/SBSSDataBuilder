using SBSSData.Softball.Common;

namespace SBSSData.Application.Support
{
    public record StatisticsDisplay(string Stat, 
                                    double Min, 
                                    double Max, 
                                    double Mean, 
                                    double Median, 
                                    double Variance, 
                                    double StdDev, 
                                    int Count) 
    {
        public StatisticsDisplay(DescriptiveStatistics ds) : this(ds.Title,
                                                                  ds.Minimum,
                                                                  ds.Maximum,
                                                                  ds.Mean,
                                                                  ds.Median,
                                                                  ds.Variance,
                                                                  ds.StdDev,
                                                                  ds.Count)
        { }
    }
}
