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

        /// <summary>
        /// Overrides <see cref="ToString()"/> to provide a text summary description of instance.
        /// </summary>
        /// <returns>
        /// The format is "[Name] ([IsHome], [RunsScored],  [Outcome]" 
        ///  where those enclosed in "[ ]" are the current values of the properties. For example,
        /// <example>
        /// The Charles Company (Visitors, Runs 9, Loss)
        /// </example>
        /// </returns>
        public override string ToString()
        {
            StringBuilder summary = new();
            string isHome = HomeTeam ? "Home" : "Visitors";
            summary.Append(Name).Append($" ({isHome}, Runs {RunsScored}, {Outcome})");

            return summary.ToString();
        }
    }
}
