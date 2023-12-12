using System.Reflection;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// The aggregate and cumulative player stats from a single game for specified team 
    /// </summary>
    public class TeamPlayerStats
    {
        /// <summary>
        /// Constructs an empty instance, that is one where the <see cref="Team"/> property is the empty object.
        /// </summary>
        private TeamPlayerStats()
        {
            Team = new Team();
        }

        /// <summary>
        /// Constructs and instance that sets the <see cref="Team"/> property.
        /// </summary>
        /// <param name="team">The team object, if <c>null</c>, the empty <c>Team</c> object is used.</param>
        public TeamPlayerStats(Team team)
        {
            Team = team ?? new();
        }

        /// <summary>
        /// Gets and initializes the <c>Team</c> property.
        /// </summary>
        /// <remarks>
        /// Use the <see cref="TeamPlayerStats(Team)"/> constructor to initialize the <c>Team property</c>.
        /// </remarks>
        public Team Team
        {
            get;
            init;
        }

        /// <summary>
        /// Returns the cumulative <see cref="PlayerStats"/> from all the players <see cref="TeamPlayerStats.Team"/> property.
        /// </summary>
        /// <remarks>
        /// Although the <see cref="GetPlayersStats()"/> method is called, saving the results in this property
        /// allow the information to be serialized.
        /// </remarks>
        public PlayerStats PlayersStats => GetPlayersStats();

        /// <summary>
        /// Gets the aggregated and calculated summary player stats for the <see cref="Team"/>.
        /// </summary>
        /// <returns>A <see cref="PlayerStats"/> instance. <c>null</c> is never returned.</returns>
        private PlayerStats GetPlayersStats()
        {
            Player player = Player.ConstructPlayer(Enumerable.Empty<PlayerLabelValue>());
            IEnumerable<Player> players = Team.Players;
            if ((players != null) && players.Any())
            {
                PropertyInfo[] playerProperties = typeof(Player).GetProperties();
                PropertyInfo playerName = playerProperties.Single(p => p.Name == "Name");
                playerName.SetValue(player, Team.Name);
                IEnumerable<PropertyInfo> properties = playerProperties.Where(p => p.PropertyType == typeof(int));
                foreach (PropertyInfo property in properties)
                {
#pragma warning disable CS8605 // Unboxing a possibly null value.
                    property.SetValue(player, players.Select(p => (int)property.GetValue(p)).ToList().Sum());
#pragma warning restore CS8605 // Unboxing a possibly null value.
                }
            }

            return new PlayerStats(player);
        }

    }
}
