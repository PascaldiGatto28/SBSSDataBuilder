using SBSSData.Softball.Common;

namespace SBSSData.Softball.Stats
{
    public record LeaguesAndPlayersStatistics(IEnumerable<LeagueStatistics> LeagueStatistics, Dictionary<string, IEnumerable<PlayerSheetPercentile>> PlayerStatistics)
    {
    }

    public record LeagueStatistics(string StatName, double Minimum, double Maximum, double Mean, double Variance, double StdDev, int Count)
    {
        public LeagueStatistics(DescriptiveStatistics ds) : this(ds.Title, ds.Minimum, ds.Maximum, ds.Mean, ds.Variance, ds.StdDev, ds.Count)
        {
        }
    }
}
