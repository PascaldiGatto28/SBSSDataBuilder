using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record LeaguePlayersStatistics(LeagueName League,
                                          IEnumerable<PlayerStatistics> PlayersStatistics,
                                          IEnumerable<StatisticsDisplay> PropertiesStatistics)
    {
    }
}
