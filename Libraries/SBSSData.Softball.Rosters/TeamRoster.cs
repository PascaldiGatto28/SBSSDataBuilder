using System.Collections.Generic;

namespace SBSSData.Softball.Rosters
{
    /// <summary>
    /// Represents a team roster containing the team's name, its manager, and the list of players.
    /// </summary>
    public class TeamRoster
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRoster"/> class with empty default values.
        /// </summary>
        /// <remarks>
        /// <para>
        /// After construction, <see cref="TeamName"/> and <see cref="Manager"/> are empty strings,
        /// and <see cref="Players"/> is an empty list. This ensures consumers can safely add to
        /// <see cref="Players"/> without checking for null.
        /// </para>
        /// </remarks>
        public TeamRoster()
        {
            TeamName = string.Empty;
            Manager = string.Empty;
            Players = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamRoster"/> class with the specified values.
        /// </summary>
        /// <param name="teamName">The name of the team. If <c>null</c> is passed, the property will be set to an empty string.</param>
        /// <param name="manager">The name of the team's manager. If <c>null</c> is passed, the property will be set to an empty string.</param>
        /// <param name="players">The list of players. If <c>null</c> is passed, an empty list will be used.</param>
        public TeamRoster(string teamName, string manager, List<string> players)
        {
            TeamName = teamName ?? string.Empty;
            Manager = manager ?? string.Empty;
            Players = players ?? new List<string>();
        }

        /// <summary>
        /// Gets or sets the team's display name.
        /// </summary>
        /// <value>
        /// A non-null string containing the team's name. By default this is an empty string.
        /// </value>
        public string TeamName { get; set; }

        /// <summary>
        /// Gets or sets the name of the team's manager.
        /// </summary>
        /// <value>
        /// A non-null string containing the manager's name. By default this is an empty string.
        /// </value>
        public string Manager { get; set; }

        /// <summary>
        /// Gets or sets the list of player names on the roster.
        /// </summary>
        /// <value>
        /// A non-null <see cref="List{String}"/> containing player names. The list may be empty but will not be null
        /// when using the provided constructors.
        /// </value>
        public List<string> Players { get; set; }
    }
}
