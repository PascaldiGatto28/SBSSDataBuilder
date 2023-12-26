namespace SBSSData.Softball.Stats
{
    public class PlayerDataSummary
    {
        public PlayerDataSummary(IEnumerable<Player> players, string name = "")
        {
            Player firstPlayer = players.First();
            PlayersData = Query.GetSummaryData(players, name);
            PlayersStats = new PlayerStats(PlayersData);
        }

        public Player PlayersData
        {
            get;
            set;
        }

        public PlayerStats PlayersStats
        {
            get;
            set;
        }
    }
}
