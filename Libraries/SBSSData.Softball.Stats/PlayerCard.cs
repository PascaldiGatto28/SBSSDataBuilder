
namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// Encapsulates the data for constructing a player card, that is, a summary of all stats data for an active player.
    /// </summary>
    public class PlayerCard
    {
        /// <summary>
        /// The private property is used to query the data store to populate the properties of the class.
        /// </summary>
        private readonly Query query = Query.Empty;

        /// <summary>
        /// The default constructor that creates an instance having all properties set to their initial and default values.
        /// </summary>
        public PlayerCard()
        {
            PlayerName = string.Empty;
            //ImageData = string.Empty;
            Leagues = Enumerable.Empty<LeagueDescription>();
            Games = Enumerable.Empty<Game>();
            Teams = Enumerable.Empty<Team>();
            query = Query.Empty;
        }

        public PlayerCard(DataStoreContainer dsContainer, string playerName) : this()
        {
            PlayerName = playerName;
            query = new Query(dsContainer);
            Initialize();
        }

        /// <summary>
        /// The player name; it is of the form "lastName, firstName", for example "Auster, Paul".
        /// </summary>
        public string PlayerName
        {
            get;
            set;
        }

        public string Season => query.GetSeason();


        /// <summary>
        /// Gets and sets the sequence of (<see cref="LeagueDescription"/>) leagues in which the player has played during the 
        /// current season.
        /// </summary>
        public IEnumerable<LeagueDescription> Leagues
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the sequence of (<see cref="Game"/>) games in which the player has played during the current season.
        /// </summary>
        public IEnumerable<Game> Games
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the sequence of (<see cref="Team"/>) teams in which the player has played at least one game during the
        /// current season.
        /// </summary>
        public IEnumerable<Team> Teams
        {
            get;
            set;
        }

        private void Initialize()
        {
            IEnumerable<Game> playedGames = query.GetPlayedGames();
            IEnumerable<Game> playerPlayedGames = playedGames.Where(g => g.Teams.SelectMany(t => t.Players)
                                                                                .Where(p => p.Name == PlayerName)
                                                                                .Any());
            Games = playerPlayedGames;

            IEnumerable<string> leagueCategories = Games.Select(g => g.GameInformation.LeagueCategory).Distinct();

            IEnumerable<Tuple<string, string>> leagueNames = Games.Select(g => Tuple.Create(g.GameInformation.LeagueCategory, g.GameInformation.LeagueDay)).Distinct();
            IEnumerable<LeagueDescription> descriptions = query.GetLeagueDescriptions().Where(l => leagueNames.Contains(Tuple.Create(l.LeagueCategory, l.LeagueDay)));

            Leagues = descriptions;

            IEnumerable<Team> teams = Games.SelectMany(g => g.Teams.Where(t => t.Players.Select(p => p.Name).Contains(PlayerName)));
            IEnumerable<IGrouping<string, Team>> groups = teams.GroupBy(t => t.Name);

            IEnumerable<Team> teamGroups = groups.Select(g => new TeamSummary(g));

            Teams = teamGroups;

        }

    }
}
