using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record TeamSummaryStatsDisplay(string Name, int Games, int Wins, int Losses, int RS, int RA, int Hits, IEnumerable<PlayerStatsDisplay> Players)
    {
        public TeamSummaryStatsDisplay(TeamSummaryStats teamSummaryStats) :
            this(
                 teamSummaryStats.Name,
                 teamSummaryStats.NumGames,
                 teamSummaryStats.NumWins,
                 teamSummaryStats.NumLosses,
                 teamSummaryStats.RunsScored,
                 teamSummaryStats.RunsAgainst,
                 teamSummaryStats.Hits,
                 teamSummaryStats.Players.Select(p => new PlayerStatsDisplay((PlayerStats)p))
                )
        {
        }
    }
}
