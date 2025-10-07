namespace SBSSData.Softball.Rosters
{
    public class TeamRoster
    {
        public TeamRoster()
        {
            TeamName = string.Empty;
            Manager = string.Empty;
            Players = [];
        }

        public TeamRoster(string teamName, string manager, List<string> players)
        {
            TeamName = teamName;
            Manager = manager;
            Players = players;
        }

        public string TeamName
        {
            get; set;
        }
        public string Manager
        {
            get; set;
        }
        public List<string> Players
        {
            get; set;
        }
    }
}
