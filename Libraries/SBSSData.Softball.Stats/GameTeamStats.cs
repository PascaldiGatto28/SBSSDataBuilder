namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// The aggregate and cumulative player stats for the each team in a game a completed game.
    /// </summary>
    public class GameTeamStats
    {
        /// <summary>
        /// Constructs an empty instance, that is, one with the <see cref="Game"/> property set to the "empty" game.
        /// </summary>
        private GameTeamStats()
        {
            Game = Game.Empty;
        }

        /// <summary>
        /// Constructs an instance where <see cref="Game"/> property is set to a value. 
        /// </summary>
        /// <remarks>
        /// It is the team information from <c>Game</c> object that is used to produce the aggregate stats for the players of
        /// each game.
        /// </remarks>
        /// <param name="game">The game, whose teams are used. If <c>null</c>, the "empty" game is used.</param>
        public GameTeamStats(Game game) : this()
        {
            Game = game ?? Game.Empty;
        }

        /// <summary>
        /// Gets and initializes the <see cref="Game"/> instance.
        /// </summary>
        /// <remarks>
        /// This property is initialized by the <see cref="GameTeamStats(Game)"/> constructor.
        /// </remarks>
        public Game Game
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the <see cref="PlayerStats"/> for the players of each team.
        /// </summary>
        /// <remarks>
        /// Although the property just access the <see cref="GameTeamStats.PlayersStats"/> property for each team, using a property
        /// allows serialization of the instance to contain the information.
        /// </remarks>
        public IEnumerable<PlayerStats> PlayersStats => GetPlayersStats();

        /// <summary>
        /// Returns a sequence of <see cref="PlayerStats"/>, an element for each <see cref="Team"/> which the 
        /// cumulative stats for both teams in the <see cref="Game"/>.
        /// </summary>
        /// <returns>The sequence of <c>PlayerStats</c>. If the game is not complete or has been canceled, the empty
        /// sequence is returned; <c>null</c> is never returned.</returns>
        private IEnumerable<PlayerStats> GetPlayersStats()
        {
            List<PlayerStats> stats = new();
            foreach (Team team in Game.Teams)
            {
                stats.Add(new TeamPlayerStats(team).PlayersStats);
            }

            return stats;
        }

    }
}
