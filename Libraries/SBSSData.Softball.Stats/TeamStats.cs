using System.Reflection;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// The aggregate and cumulative player stats from a single game for specified team.
    /// </summary>
    public class TeamStats : Team
    {
        /// <summary>
        /// Constructs an empty instance, that is one where the properties all have default values.
        /// </summary>
        private TeamStats()
        {

        }

        /// <summary>
        /// Creates a instance using the data from the <paramref name="team"/> parameter.
        /// </summary>
        /// <param name="team">A non-null <see cref="Team"/> instance.</param>
        /// <remarks>
        /// This subclass differs from the base class in that the list of <see cref="Player"/> objects is
        /// replaced by a list of <see cref="PlayerStats"/> objects, essentially adding computed stats to the
        /// <c>Player</c> objects. It also add a <c>PlayerStats</c> object to the <see cref="Team.Players"/> list
        /// that is a summary of all players and represents the stats date for the team.
        /// <para>
        /// If <paramref name="team"/> is <c>null</c> an "empty" (all properties are default values) 
        /// instance is created.
        /// </para>
        /// </remarks>
        public TeamStats(Team team) : base(team)
        {
            List<Player> playersStats = team.Players.Select(p => new PlayerStats(p)).Cast<Player>().ToList();
            PlayerStats summaryPlayerStats = GetPlayersStats(team);
            playersStats.Add(summaryPlayerStats);
            Players = playersStats.Cast<Player>().ToList();
        }

        /// <summary>
        /// Gets the aggregated and calculated summary player stats for the <see cref="Team"/>.
        /// </summary>
        /// <returns>A <see cref="PlayerStats"/> instance. <c>null</c> is never returned.</returns>
        private PlayerStats GetPlayersStats(Team team)
        {
            Player player = Player.ConstructPlayer(Enumerable.Empty<PlayerLabelValue>());
            IEnumerable<Player> players = team.Players;
            if ((players != null) && players.Any())
            {
                PropertyInfo[] playerProperties = typeof(Player).GetProperties();
                PropertyInfo playerName = playerProperties.Single(p => p.Name == "Name");
                playerName.SetValue(player, team.Name);
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
