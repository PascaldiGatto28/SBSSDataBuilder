namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// Encapsulates the data for constructing a player card, that is, a summary of all stats data for an active player.
    /// </summary>
    public class PlayerCardV2
    {
        ///// <summary>
        ///// The private property is used to query the data store to populate the properties of the class.
        ///// </summary>
        //private readonly Query query = Query.Empty;

        /// <summary>
        /// The default constructor that creates an instance having all properties set to their initial and default values.
        /// </summary>
        public PlayerCardV2()
        {
            PlayerName = string.Empty;
            //ImageData = string.Empty;
            Leagues = Enumerable.Empty<LeagueDescription>();
            Games = Enumerable.Empty<Game>();
            Teams = Enumerable.Empty<Team>();
            //query = Query.Empty;
        }

        public PlayerCardV2(DataStoreContainer dsContainer, string playerName) : this()
        {
            PlayerName = playerName;
            //query = new Query(dsContainer);
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

        public string Season
        {
            get; set;
        }


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
            IEnumerable<Game> playedGames = Enumerable.Empty<Game>(); // query.GetPlayedGames();
            IEnumerable<Game> playerPlayedGames = playedGames.Where(g => g.Teams.SelectMany(t => t.Players)
                                                                                .Where(p => p.Name == PlayerName)
                                                                                .Any());
            Games = playerPlayedGames;

            IEnumerable<string> leagueCategories = Games.Select(g => g.GameInformation.LeagueCategory).Distinct();

            IEnumerable<Tuple<string, string>> leagueNames = Games.Select(g => Tuple.Create(g.GameInformation.LeagueCategory, g.GameInformation.LeagueDay)).Distinct();
            IEnumerable<LeagueDescription> descriptions = Enumerable.Empty<LeagueDescription>();// query.GetLeagueDescriptions().Where(l => leagueNames.Contains(Tuple.Create(l.LeagueCategory, l.LeagueDay)));

            Leagues = descriptions;

            IEnumerable<Team> allTeams = Games.SelectMany(g => g.Teams);
            IEnumerable<string> teams = allTeams.Where(t => t.Players.Select(p => p.Name).Contains(PlayerName)).Select(t => t.Name);
            IEnumerable<IGrouping<string, Team>> groups = allTeams.Where(t => teams.Contains(t.Name)).GroupBy(t => t.Name);

            IEnumerable<Team> teamGroups = groups.Select(g => new TeamSummary(g));

            Teams = teamGroups;

        }

    }
}
