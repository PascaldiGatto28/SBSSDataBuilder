using Utility = SBSSData.Softball.Stats.StatsUtilities;

namespace SBSSData.Softball.Stats
{
    public class TeamSummaryStats : Team
    {
        public TeamSummaryStats() : base()
        {
        }

        public TeamSummaryStats(IEnumerable<Team> teams) : this()
        {
            NumGames = teams.Count();
            Name = teams.First().Name;
            Utility.SumIntProperties<Team>(teams, this);

            NumWins = teams.Where(t => t.Outcome == "Win").Count();
            Outcome = $"{NumWins} wins and {NumLosses} losses";
            NumHomeGames = teams.Where(t => t.HomeTeam).Count();

            List<Player> playerList = new();

            IEnumerable<IGrouping<string, Player>> playerGroups = teams.SelectMany(t => t.Players).GroupBy(p => p.Name);
            foreach (IGrouping<string, Player> playerGroup in playerGroups)
            {
                Player player = Player.ConstructPlayer(new List<PlayerLabelValue> { new("Player", playerGroup.Key) });
                playerGroup.ToList().SumIntProperties<Player>(player);
                playerList.Add(new PlayerStats(player, playerGroup.ToList().Count));
            }

            playerList = playerList.Cast<PlayerStats>().OrderByDescending(p => p.NumGames).Cast<Player>().ToList();
            Player summary = Player.ConstructPlayer(new List<PlayerLabelValue> { new("Player", Name) });
            playerList.SumIntProperties<Player>(summary);

            playerList.Add(new PlayerStats(summary, NumGames));
            Players = playerList;
        }

        public int NumGames
        {
            get;
            set;
        }

        public int NumHomeGames
        {
            get;
            set;
        }


        public int NumWins
        {
            get;
            set;
        }

        public int NumLosses => NumGames - NumWins;

    }
}
