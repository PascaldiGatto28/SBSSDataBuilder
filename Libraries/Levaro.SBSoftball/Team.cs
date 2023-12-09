using System.Text;

namespace Levaro.SBSoftball
{
    /// <summary>
    /// Encapsulates team information, stats and all players stats for a game.
    /// </summary>
    public sealed class Team
    {
        /// <summary>
        /// Constructs an "empty" instance of this class.
        /// </summary>
        /// <remarks>
        /// Instances should only be created using the <see cref="Game.ConstructTeams(HtmlAgilityPack.HtmlDocument)"/> static
        /// method. That method actually produces instance of both teams for a game. Instances should really be in pairs
        /// because they are part of a single game.
        /// </remarks>
        public Team()
        {
            Name = "Unknown";
            Outcome = "Unknown";
            Players = Enumerable.Empty<Player>().ToList();
        }

        /// <summary>
        /// Gets and initializes the team name.
        /// </summary>
        public string Name
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes a boolean value of <c>true</c> if this is the home; else <c>false</c>.
        /// </summary>
        public bool HomeTeam
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of runs scored in this game.
        /// </summary>
        public int RunsScored
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of runs scored against the team in this game.
        /// </summary>
        /// <remarks>
        /// This value is the as the <see cref="RunsScored"/> for the opposing team.
        /// </remarks>
        public int RunsAgainst
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and initialized the number of hits produced by this team for the game.
        /// </summary>
        public int Hits
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the value that indicates the outcome; "Win" if the team won or "Loss" if it did not.
        /// </summary>
        public string Outcome
        {
            get;
            init;
        }

        /// <summary>
        /// A sequence of <see cref="Player"/> objects containing the name of stats for each player participating for
        /// the team in this game.
        /// </summary>
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
