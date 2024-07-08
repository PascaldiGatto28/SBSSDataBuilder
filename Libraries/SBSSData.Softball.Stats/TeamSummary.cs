using Utility = SBSSData.Softball.Stats.StatsUtilities;

namespace SBSSData.Softball.Stats
{
    public class TeamSummary : Team
    {
        public TeamSummary() : base()
        {
        }

        public TeamSummary(IEnumerable<Team> teams, string name = "") : this()
        {
            NumGames = teams.Count();
            Name = string.IsNullOrEmpty(name) ? teams.First().Name : name;
            Utility.SumIntProperties<Team>(teams, this);

            NumWins = teams.Where(t => t.Outcome == "Win").Count();
            Outcome = $"{NumWins} wins and {NumLosses} losses";
            NumHomeGames = teams.Where(t => t.HomeTeam).Count();

            List<Player> playerList = [];

            // Get all the players, but "summary" players should not be included. Real players always name with a comma to 
            // separate first and last name. There are no team names that have comma character within. (This needs to be 
            // fixed, because a team name could have a comma in it I suppose.)
            IEnumerable<IGrouping<string, Player>> playerGroups = teams.SelectMany(t => t.Players)
                                                                       .Where(p => p.Name.Contains(','))
                                                                       .GroupBy(p => p.Name);
            foreach (IGrouping<string, Player> playerGroup in playerGroups)
            {
                Player player = Player.ConstructPlayer(new List<PlayerLabelValue> { new("Player", playerGroup.Key) });
                playerGroup.ToList().SumIntProperties<Player>(player);
                playerList.Add(player);
            }

            //playerList = playerList.Cast<PlayerStats>().OrderByDescending(p => p.NumGames).Cast<Player>().ToList();
            Player summary = Player.ConstructPlayer(new List<PlayerLabelValue> { new("Player", Name) });
            playerList.SumIntProperties<Player>(summary);
            playerList.Add(summary);

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
