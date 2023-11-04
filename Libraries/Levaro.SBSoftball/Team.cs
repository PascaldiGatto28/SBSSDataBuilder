using System.Text;

namespace Levaro.SBSoftball
{
    public class Team
    {
        public Team()
        {
            Name = string.Empty;
            Outcome = string.Empty;
            Players = Enumerable.Empty<Player>().ToList();
        }

        public string Name
        {
            get;
            set;
        }

        public bool HomeTeam
        {
            get;
            set;
        }

        public int RunsScored
        {
            get;
            set;
        }

        public int RunsAgainst
        {
            get;
            set;
        }

        public int Hits
        {
            get;
            set;
        }
        public string Outcome
        {
            get;
            set;
        }
        public List<Player> Players
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder summary = new();
            //string indent = new string(' ', 4);
            string isHome = HomeTeam ? "Home" : "Visitors";
            summary.Append(Name).Append($" ({isHome}, Runs {RunsScored}, {Outcome})");

            return summary.ToString();
        }
    }
}
