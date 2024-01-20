namespace SBSSData.Softball.Stats
{
    public record TeamStatsDisplay(string Name, int RunsScored, int RunsAgainst, int Hits, string Outcome, IEnumerable<PlayerStatsDisplay> Players)
    {
        public TeamStatsDisplay(TeamStats teamStats) :
            this(
                 teamStats.Name,
                 teamStats.RunsScored,
                 teamStats.RunsAgainst,
                 teamStats.Hits,
                 teamStats.Outcome,
                 teamStats.Players.Select(p => new PlayerStatsDisplay((PlayerStats)p))
                )
        {
        }

        public override string ToString()
        {
            return $"{Name} RS:{RunsScored} RA:{RunsAgainst} Hits:{Hits} Outcome:{Outcome}";
        }

    }



}
