using System.Reflection;

using SBSSData.Softball.Common;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// Query provides read-only query operations over a <see cref="DataStoreContainer"/> containing
    /// softball league data. This class exposes helpers to retrieve league descriptions, schedules,
    /// player and team statistics, and descriptive statistics for computed metrics.
    /// </summary>
    /// <remarks>
    /// Instances are typically constructed with a <see cref="DataStoreContainer"/> that wraps a
    /// <see cref="LeaguesData"/> object. A lightweight <see cref="Empty"/> instance is available.
    /// The class contains a cache-like dictionary of valid league categories and their days which
    /// is initialized when an instance is created with a non-empty container.
    /// </remarks>
    public class Query
    {
        /// <summary>
        /// A dictionary mapping league category names to the list of valid days for that category.
        /// </summary>
        private static Dictionary<string, List<string>> validLeaguesDictionary = [];

        /// <summary>
        /// Names of computed player statistics that are used by the query methods.
        /// </summary>
        public static readonly List<string> ComputedStatNames = ["Average", "Slugging", "OnBase", "OnBasePlusSlugging"];

        /// <summary>
        /// Display names corresponding to <see cref="ComputedStatNames"/>.
        /// </summary>
        public static readonly List<string> ComputedStatDisplayNames = ["AVG", "SLG", "OBP", "OPS"];

        /// <summary>
        /// Private default constructor that constructs a Query with an empty container.
        /// </summary>
        private Query()
        {
            Container = DataStoreContainer.Empty;
        }

        /// <summary>
        /// Creates a new <see cref="Query"/> instance that operates on the provided <paramref name="dsContainer"/>.
        /// </summary>
        /// <param name="dsContainer">The data store container to use for queries. If <c>null</c>, an empty container is used.</param>
        public Query(DataStoreContainer dsContainer)
        {
            Container = dsContainer ?? DataStoreContainer.Empty;
            validLeaguesDictionary = this.ValidLeagueDescriptions();
        }

        /// <summary>
        /// Returns an empty <see cref="Query"/> instance with an empty data container.
        /// </summary>
        public static Query Empty => new();

        /// <summary>
        /// The data store container used by this query instance.
        /// </summary>
        public DataStoreContainer Container
        {
            get;
            set;
        }

        /// <summary>
        /// Convenience accessor for the underlying <see cref="LeaguesData"/> instance in the container.
        /// </summary>
        public LeaguesData DataStore => Container.DataStore;


        /// <summary>
        /// Returns the sequence of <see cref="LeagueDescription"/> objects from the data store.
        /// </summary>
        /// <returns>An enumerable of league descriptions.</returns>
        public IEnumerable<LeagueDescription> GetLeagueDescriptions() => DataStore.LeagueSchedules.Select(s => s.LeagueDescription);

        //public IEnumerable<LeagueName> GetLeagueNames() => GetLeagueDescriptions().OrderBy(d => d.LeagueCategory).Select(l => new LeagueName(l)).ToList();

        /// <summary>
        /// Returns league names optionally limited to the provided <paramref name="leagueDescription"/> set.
        /// </summary>
        /// <param name="leagueDescription">Optional sequence of <see cref="LeagueDescription"/> instances to convert to <see cref="LeagueName"/>s.
        /// If <c>null</c>, all league descriptions are used.</param>
        /// <returns>An ordered list of <see cref="LeagueName"/> objects.</returns>
        public IEnumerable<LeagueName> GetLeagueNames(IEnumerable<LeagueDescription>? leagueDescription = null)
        {

            IEnumerable<LeagueName> leagueNames = [];
            IEnumerable<LeagueDescription> descriptions = leagueDescription ?? GetLeagueDescriptions();
            return descriptions.OrderBy(d => d.LeagueCategory).Select(l => new LeagueName(l)).ToList();
        }

        /// <summary>
        /// Computes player sheet percentiles and z-scores for all players in a league for the computed stat names.
        /// </summary>
        /// <param name="league">The league to compute statistics for.</param>
        /// <param name="qualifyingPlateAppearances">Minimum plate appearances required to be included in percentile calculations.</param>
        /// <returns>
        /// A dictionary keyed by player name whose values are enumerables of <see cref="PlayerSheetPercentile"/>
        /// objects representing each computed stat's percentile and related metrics for that player.
        /// </returns>
        public Dictionary<string, IEnumerable<PlayerSheetPercentile>> GetAllPlayersStatistics(LeagueName league,
                                                                                             int qualifyingPlateAppearances)
        {
            IEnumerable<IGrouping<string, PlayerSheetPercentile>> groups = [];

            List<PlayerStats> psAll = GetLeaguePlayers(league.Category, league.Day).ToList();
            List<double> weights = psAll.Select(p => (double)p.PlateAppearances).ToList();

            List<PlayerStats> ps = GetLeaguePlayers(league.Category, league.Day).Where(p => p.PlateAppearances > qualifyingPlateAppearances)
                                                                                .ToList();
            int n = ps.Count;
            //List<string> propertyNames = ["Average", "Slugging", "OnBase", "OnBasePlusSlugging"];
            List<PlayerSheetPercentile> playersInfo = [];
            foreach (string propertyName in ComputedStatNames)
            {
                PropertyInfo? property = typeof(PlayerStats).GetProperty(propertyName);
                if (property != null)
                {
                    List<double> data = psAll.Select(p => property.GetValue(p)).Cast<double>().ToList();
                    DescriptiveStatistics ds = data.GetStatistics(propertyName, weights);
                    List<PlayerStats> propertyNameStats = ps.OrderBy(p => property.GetValue(p)).ThenBy(p => p.PlateAppearances).ToList();
                    for (int i = 0; i < n; i++)
                    {
                        int percentile = (int)Math.Round(((double)((i == 0) ? 0 : i) / (double)n) * 100, 0);
                        int rank = n - i;
                        double propertyValue = (double)(property.GetValue(propertyNameStats[i]) ?? 0);
                        double zScore = (propertyValue - ds.Mean) / ds.StdDev;
                        PlayerSheetPercentile pInfo = new(n, propertyNameStats[i].Name, propertyName, (double)(property.GetValue(propertyNameStats[i]) ?? 0), rank, percentile, zScore);
                        playersInfo.Add(pInfo);
                    }
                }
            }

            groups = playersInfo.GroupBy(i => i.PlayerName);
            return groups.ToDictionary(g => g.Key, g => g.AsEnumerable<PlayerSheetPercentile>());
        }

        /// <summary>
        /// Computes descriptive statistics for each league for the configured computed stat names.
        /// </summary>
        /// <returns>A dictionary keyed by short league name with values being enumerables of <see cref="DescriptiveStatistics"/>.</returns>
        public Dictionary<string, IEnumerable<DescriptiveStatistics>> GetLeaguesStatistics()
        {
            Dictionary<string, IEnumerable<DescriptiveStatistics>> leagueStatistics = [];
            foreach (LeagueName leagueName in GetLeagueNames())
            {
                List<PlayerStats> psAll = GetLeaguePlayers(leagueName.Category, leagueName.Day).ToList();
                List<double> weights = psAll.Select(p => (double)p.PlateAppearances).ToList();
                List<DescriptiveStatistics> descriptiveStatistics = [];
                foreach (string propertyName in ComputedStatNames)
                {
                    PropertyInfo? property = typeof(PlayerStats).GetProperty(propertyName);
                    if (property != null)
                    {
                        List<double> data = psAll.Select(p => (double)(property.GetValue(p) ?? 0)).ToList();
                        DescriptiveStatistics ds = data.GetStatistics(propertyName, weights);
                        descriptiveStatistics.Add(ds);
                    }
                }

                leagueStatistics.Add(leagueName.ShortLeagueName, descriptiveStatistics);
            }

            return leagueStatistics;
        }

        /// <summary>
        /// Get league names for every league for which a player has played in a game in that league.
        /// </summary>
        /// <param name="playerName">The player name must be in the format "LastName, FirstName".</param>
        /// <returns>A list of <see cref="LeagueName"/> objects representing leagues the specified player has appeared in.</returns>
        public IEnumerable<LeagueName> GetLeagueNamesForPlayer(string playerName)
        {
            IEnumerable<LeagueName> leagueNames = [];
            if (!string.IsNullOrEmpty(playerName))
            {
                IEnumerable<Game> playerPlayedGames = GetPlayedGames().Where(g => g.Teams.SelectMany(t => t.Players).Any(p => p.Name == playerName));
                leagueNames = playerPlayedGames.Select(g => g.GameInformation)
                                               .Select(i => new LeagueName(i))
                                               .Distinct()
                                               .OrderBy(n => n.Category)
                                               .ToList();
            }

            return leagueNames;
        }

        /// <summary>
        /// Returns a human readable season string for the first league description in the data store.
        /// </summary>
        /// <returns>A string in the format "{Season} {Year}" for the first league description.</returns>
        /// <remarks>If no league descriptions exist this may throw an exception when calling <see cref="First"/>.</remarks>
        public string GetSeason()
        {
            LeagueDescription league = GetLeagueDescriptions().First();
            return $"{league.Season} {league.Year}";
        }

        /// <summary>
        /// Returns the enumerable of scheduled games held in the configured <see cref="Container"/>.
        /// </summary>
        public IEnumerable<ScheduledGame> ScheduledGames => Container.GetScheduledGames();


        /// <summary>
        /// Returns the sequence of <see cref="Game"/> objects for scheduled games that are complete and not canceled.
        /// </summary>
        /// <returns>An enumerable of completed, non-canceled games containing results.</returns>
        public IEnumerable<Game> GetPlayedGames() => ScheduledGames.Where(s => s.IsComplete && !s.WasCanceled)
                                                                   .Select(s => s.GameResults)
                                                                   .Where(s => !s.IsForfeited);

        /// <summary>
        /// Returns all played games (Game results) for the specified league filters.
        /// </summary>
        /// <param name="leagueCategory">Optional league category filter. Default empty string for no filter.</param>
        /// <param name="day">Optional day filter. Default empty string for no filter.</param>
        /// <returns>An enumerable of <see cref="Game"/> for matching scheduled games that were completed and not canceled or forfeited.</returns>
        public IEnumerable<Game> GetLeaguePlayedGames(string leagueCategory = "", string day = "")
        {
            return GetLeagueSchedules(leagueCategory, day).SelectMany(l => l.ScheduledGames)
                                                          .Where(s => s.IsComplete && !s.WasCanceled && !s.GameResults.IsForfeited)
                                                          .Select(s => s.GameResults);


        }

        /// <summary>
        /// Returns active players who have participated in completed, non-canceled games.
        /// </summary>
        /// <returns>An ordered enumerable of <see cref="Player"/> instances representing active players.</returns>
        public IEnumerable<Player> GetActivePlayers() => GetPlayedGames().SelectMany(g => g.Teams)
                                                                         .SelectMany(t => t.Players)
                                                                         .OrderBy(p => p.Name).ToList();

        /// <summary>
        /// Returns the names of active players, distinct and ordered.
        /// </summary>
        /// <returns>An enumerable of active player names as strings.</returns>
        public IEnumerable<string> GetActivePlayerNames() =>  GetActivePlayers().Select(p => p.Name).Distinct().OrderBy(p => p);

        /// <summary>
        /// Returns player statistics aggregated for a league filtered by category and day.
        /// </summary>
        /// <param name="leagueCategory">Optional league category filter. Empty string means no filtering.</param>
        /// <param name="day">Optional day filter. Empty string means no filtering.</param>
        /// <returns>An enumerable of <see cref="PlayerStats"/> aggregated per player for the matching league schedule(s).</returns>
        public IEnumerable<PlayerStats> GetLeaguePlayers(string leagueCategory = "", string day = "")
        {
            IEnumerable<PlayerStats> leaguePlayers = GetLeagueSchedules(leagueCategory, day)
                                          .SelectMany(l => l.ScheduledGames)
                                          .Where(s => s.IsComplete && !s.WasCanceled && !s.GameResults.IsForfeited)
                                          .Select(s => s.GameResults)
                                          .SelectMany(g => g.Teams)
                                          .SelectMany(t => t.Players)
                                          .GroupBy(p => p.Name)
                                          .Select(gp => new
                                          {
                                              gp.Key,
                                              Player = new PlayerStats(GetSummaryData(gp.ToList()), gp.ToList().Count)
                                          }).OrderByDescending(p => p.Player.NumGames).ThenByDescending(p => p.Player.AtBats)
                                            .Select(p => p.Player);
            return leaguePlayers;
        }

        /// <summary>
        /// Returns active player statistics converted to <see cref="PlayerStats"/> objects.
        /// </summary>
        public IEnumerable<PlayerStats> PlayersStats => GetActivePlayers().Select(p => new PlayerStats(p));

        /// <summary>
        /// Returns the player statistics for a league and an appended summary row containing totals across the league.
        /// </summary>
        /// <param name="leagueCategory">Optional league category filter.</param>
        /// <param name="day">Optional day filter.</param>
        /// <returns>An enumerable of <see cref="PlayerStats"/> including a totals summary item appended at the end.</returns>
        public IEnumerable<PlayerStats> GetLeaguePlayersSummary(string leagueCategory = "", string day = "")
        {
            IEnumerable<PlayerStats> leaguePlayers = GetLeaguePlayers(leagueCategory, day).OrderByDescending(s => s.PlateAppearances);
            IEnumerable<LeagueSchedule> leagueSchedules = GetLeagueSchedules(leagueCategory, day);
            string summaryName = leagueSchedules.Select(s => $"{s.LeagueDescription.ToShortString()} Totals").ToString("\r\n");
            int numGames = leagueSchedules.SelectMany(l => l.ScheduledGames).Where(s => s.IsComplete && !s.WasCanceled).Count();

            PlayerStats summaryStats = new(GetSummaryData(leaguePlayers, summaryName))
            {
                NumGames = numGames
            };

            leaguePlayers = leaguePlayers.Append(summaryStats);
            return leaguePlayers;
        }

        /// <summary>
        /// Computes rankings for players in a league based on configured fields: Average, Slugging, OnBase, OnBasePlusSlugging.
        /// </summary>
        /// <param name="leagueCategory">Optional league category filter.</param>
        /// <param name="day">Optional day filter.</param>
        /// <returns>An enumerable of <see cref="PlayerStatsRank"/> representing per-player ranks for the league.</returns>
        public IEnumerable<PlayerStatsRank> GetLeaguePlayerStatsRank(string leagueCategory = "", string day = "")
        {
            List<PlayerStatsRank> playerStatsRanks = [];
            IEnumerable<PlayerStats> ps = GetLeaguePlayersSummary(leagueCategory, day);

            string[] fieldNames = ["Average", "Slugging","OnBase", "OnBasePlusSlugging"];
            Dictionary<string, string[]> rankingsMap = [];
            foreach (PlayerStats playerStats in ps)
            {
                rankingsMap.Add(playerStats.Name, ["NA", "NA", "NA", "NA"]);
            }

            //TODO: 12 should be a parameter
            List<PlayerStats> psa = ps.Where(p => (p.PlateAppearances > 5) && (p.FirstName != string.Empty)).ToList();
            for (int j = 0; j < 4; j++)
            {
                PropertyInfo? property = typeof(PlayerStats).GetProperty(fieldNames[j]);
                if (property != null)
                {
                    List<PlayerStats> fieldNameValues = psa.OrderBy(p => property.GetValue(p)).ThenBy(p => p.PlateAppearances).ToList();
                    int n = fieldNameValues.Count;
                    for (int i = 0; i < n; i++)
                    {
                        PlayerStats player = fieldNameValues[i];
                        rankingsMap[player.Name][j] = (n - i).ToString();
                    }
                }
            }

            List<Ranking> rankingList = [];
            foreach (string key in rankingsMap.Keys)
            {
                PlayerStats player = ps.Single(p => p.Name == key);
                string[] values = rankingsMap[key];
                rankingList.Add(new Ranking(player, values[0], values[1], values[2], values[3]));
            }

            return rankingList.Select(r => new PlayerStatsRank(r.Player, new Rank(r.Average, r.Slugging, r.OnBase, r.OnBasePlusSlugging))).ToList();
        }

        /// <summary>
        /// Aggregates per-team player statistics within the specified league and day.
        /// </summary>
        /// <param name="leagueCategory">League category to filter by.</param>
        /// <param name="day">Day to filter by.</param>
        /// <returns>An ordered enumerable of <see cref="TeamSummaryStats"/> sorted by wins and run differential.</returns>
        public IEnumerable<TeamSummaryStats> GetTeamsPlayersStats(string leagueCategory, string day)
        {
            IEnumerable<Game> playedGames = GetLeagueSchedule(leagueCategory, day)
                                           .ScheduledGames
                                           .Where(s => s.IsComplete && !s.WasCanceled)
                                           .Select(s => s.GameResults);

            IEnumerable<IGrouping<string, Team>> teamGroups = playedGames.SelectMany(g => g.Teams).GroupBy(t => t.Name);

            List<TeamSummaryStats> teamPlayersStats = [];
            foreach (IGrouping<string, Team> teamGroup in teamGroups)
            {
                TeamSummaryStats teamSummary = new(teamGroup.ToList());
                teamPlayersStats.Add(teamSummary);
            }

            return teamPlayersStats.OrderByDescending(t => t.NumWins).ThenByDescending(t => t.RunsScored - t.RunsAgainst);
        }

        /// <summary>
        /// Returns the <see cref="TeamSummaryStats"/> for the specified team in the specified league/day.
        /// </summary>
        /// <param name="teamName">The team name to look up.</param>
        /// <param name="leagueCategory">League category to filter schedules.</param>
        /// <param name="day">Day to filter schedules.</param>
        /// <returns>The matching <see cref="TeamSummaryStats"/> for the team.</returns>
        public TeamSummaryStats GetTeamPlayersStats(string teamName, string leagueCategory, string day)
        {
            // TODO: Do error checking here.
            IEnumerable<TeamSummaryStats> teamsSummaryStats = GetTeamsPlayersStats(leagueCategory, day);
            TeamSummaryStats teamSummaryStats = teamsSummaryStats.Single(t => t.Name == teamName);
            return teamSummaryStats;
        }



        /// <summary>
        /// Constructs an aggregated <see cref="Player"/> instance by summing integer properties from a sequence of players.
        /// </summary>
        /// <param name="playerData">Sequence of <see cref="Player"/> instances to summarize.</param>
        /// <param name="summaryName">Optional name to assign to the constructed summary player. If empty, the first player's name is used.</param>
        /// <returns>A <see cref="Player"/> instance containing summed integer properties or <see cref="Player.Empty"/> if input is empty.</returns>
        public static Player GetSummaryData(IEnumerable<Player> playerData, string summaryName = "")
        {
            Player player = Player.Empty;
            if ((playerData != null) && playerData.Any())
            {
                string nameText = !string.IsNullOrEmpty(summaryName) ? summaryName : playerData.First().Name;
                player = Player.ConstructPlayer([new("Player", nameText)]);
                player = playerData.SumIntProperties<Player>(player);
            }

            return player;
        }

        /// <summary>
        /// Returns a sequence of <see cref="LeagueSchedule"/> objects filtered by the supplied parameters.
        /// </summary>
        /// <param name="category">Optional league category filter; empty string means no filtering on category.</param>
        /// <param name="day">Optional day filter; empty string means no filtering on day.</param>
        /// <param name="season">Optional season filter; empty string means no filtering on season.</param>
        /// <param name="year">Optional year filter; empty string means no filtering on year.</param>
        /// <returns>A filtered enumerable of <see cref="LeagueSchedule"/> objects matching the provided criteria.</returns>
        /// <remarks>
        /// Filtering respects valid combinations of category and day using the cached <see cref="validLeaguesDictionary"/>.
        /// Invalid combinations (for example a day not valid for a category) result in fewer or no schedules returned.
        /// </remarks>
        public IEnumerable<LeagueSchedule> GetLeagueSchedules(string category = "", string day = "", string season = "", string year = "")
        {
            Dictionary<string, List<string>> validLeagues = validLeaguesDictionary;
            IEnumerable<LeagueSchedule> leagueSchedules = DataStore.LeagueSchedules;
            if (!string.IsNullOrEmpty(category) && validLeagues.ContainsKey(category))
            {
                leagueSchedules = GetSchedules(leagueSchedules, "LeagueCategory", category);
            }

            if (!string.IsNullOrEmpty(day))
            {
                if (string.IsNullOrEmpty(category) || validLeagues[category].Contains(day))
                {
                    leagueSchedules = GetSchedules(leagueSchedules, "LeagueDay", day);
                }
            }

            if (!string.IsNullOrEmpty(season))
            {
                leagueSchedules = GetSchedules(leagueSchedules, "Season", season);
            }

            if (!string.IsNullOrEmpty(year))
            {
                leagueSchedules = GetSchedules(leagueSchedules, "Year", year);
            }

            return leagueSchedules;
        }

        /// <summary>
        /// Returns a single <see cref="LeagueSchedule"/> that exactly matches the provided category and day,
        /// if the combination is valid; otherwise returns an empty schedule.
        /// </summary>
        /// <param name="category">The league category to filter by.</param>
        /// <param name="day">The day to filter by.</param>
        /// <returns>The matching <see cref="LeagueSchedule"/> or <see cref="LeagueSchedule.Empty"/> if not found or invalid.</returns>
        public LeagueSchedule GetLeagueSchedule(string category, string day)
        {
            LeagueSchedule leagueSchedule = LeagueSchedule.Empty();
            if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(day)
                                                && validLeaguesDictionary.TryGetValue(category, out List<string>? value)
                                                && value.Contains(day))
            {
                leagueSchedule = GetLeagueSchedules(category, day).Single();
            }

            return leagueSchedule;
        }

        // TODO: This should be static and only executed once.
        /// <summary>
        /// Builds the dictionary of valid league descriptions mapping league category to the valid days in the data store.
        /// </summary>
        /// <returns>A dictionary mapping category names to lists of valid days found in the store.</returns>
        private Dictionary<string, List<string>> ValidLeagueDescriptions()
        {
            Dictionary<string, List<string>> validLeagues = [];
            foreach (LeagueDescription description in GetLeagueDescriptions())
            {
                string category = description.LeagueCategory;
                string day = description.LeagueDay;
                if (!validLeagues.TryGetValue(category, out List<string>? value))
                {
                    value = [];
                    validLeagues.Add(category, value);
                }

                List<string> days = value;
                if (!days.Contains(day))
                {
                    value.Add(day);
                }
            }

            return validLeagues;
        }

        /// <summary>
        /// Filters the provided <paramref name="schedules"/> by matching the string value of the named property
        /// of each schedule's <see cref="LeagueDescription"/> against <paramref name="propertyValue"/>.
        /// </summary>
        /// <param name="schedules">The schedules to filter.</param>
        /// <param name="propertyName">The property name on <see cref="LeagueDescription"/> to compare (for example "LeagueCategory").</param>
        /// <param name="propertyValue">The value to match (case-insensitive).</param>
        /// <returns>A filtered enumerable of <see cref="LeagueSchedule"/> matching the given property value.</returns>
        public static IEnumerable<LeagueSchedule> GetSchedules(IEnumerable<LeagueSchedule> schedules, string propertyName, string propertyValue)
        {
            IEnumerable<LeagueSchedule> newSchedules = schedules ?? [];
            if ((schedules != null) && schedules.Any() && !string.IsNullOrEmpty(propertyName))
            {
                PropertyInfo? property = typeof(LeagueDescription).GetProperty(propertyName);
                if (property != null)
                {
                    newSchedules = schedules.Where(l => string.Equals((string?)property.GetValue(l.LeagueDescription), propertyValue, StringComparison.OrdinalIgnoreCase));
                }

            }
            return newSchedules;
        }

        /// <summary>
        /// Returns a string representation of the query instance, delegating to the underlying container.
        /// </summary>
        /// <returns>A textual representation of the container state.</returns>
        public override string ToString()
        {
            return Container.ToString();
        }
    }
}