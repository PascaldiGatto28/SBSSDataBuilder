using System.Reflection;

using SBSSData.Softball.Common;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// TODO: Queries should allow a leagues to be specified like league players method.
    /// </summary>
    public class Query
    {
        //public readonly Dictionary<string, List<string>> ValidLeagues = new();

        public Query()
        {
            DataStore = LeaguesData.Empty;
        }

        public Query(string dsPath)
        {
            DataStore = DataStoreContainer.Instance(dsPath).DataStore;


        }

        public LeaguesData DataStore
        {
            get;
            init;
        }



        public IEnumerable<LeagueDescription> LeagueDescriptions => DataStore.LeagueSchedules.Select(s => s.LeagueDescription);

        public IEnumerable<ScheduledGame> ScheduledGames => DataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames);

        //public Dictionary<string, IEnumerable<string>> GetLeagues()
        //{
        //    Dictionary<string, IEnumerable<string>> leagues = new Dictionary<string, IEnumerable<string>>();
        //    foreach (string category in )
        //}


        public IEnumerable<Game> PlayedGames => ScheduledGames.Where(s => s.IsComplete && !s.WasCancelled).Select(s => s.GameResults);

        public IEnumerable<Player> Players => PlayedGames.SelectMany(g => g.Teams)
                                                         .SelectMany(t => t.Players)
                                                         .OrderBy(p => p.Name);

        public IEnumerable<Player> LeaguePlayers(string leagueCategory = "", string day = "")
            => GetLeagueSchedules(leagueCategory, day).SelectMany(l => l.ScheduledGames)
                                                      .Where(s => s.IsComplete && !s.WasCancelled)
                                                      .Select(s => s.GameResults)
                                                      .SelectMany(g => g.Teams)
                                                      .SelectMany(t => t.Players)
                                                      .GroupBy(p => p.Name)
                                                      .Select(gp => new
                                                      {
                                                          gp.Key,
                                                          Player = new PlayerStats(GetSummaryData(gp.ToList()), gp.ToList().Count)
                                                      }).OrderByDescending(p => p.Player.NumGames)
                                                        .ThenByDescending(p => p.Player.AtBats)
                                                        .ThenBy(p => p.Player.Name)
                                                        .Select(p => p.Player);

        public IEnumerable<PlayerStats> PlayersStats => Players.Select(p => new PlayerStats(p));

        public IEnumerable<Player> GetLeaguePlayersSummary(string leagueCategory = "", string day = "")
        {
            IEnumerable<Player> leaguePlayers = new List<Player>();
            IEnumerable<LeagueSchedule> leagueSchedules = GetLeagueSchedules(leagueCategory, day);
            string summaryName = leagueSchedules.Select(s => s.LeagueDescription).ToString<LeagueDescription>("; ");
            int numGames = leagueSchedules.SelectMany(l => l.ScheduledGames).Where(s => s.IsComplete && !s.WasCancelled).Count();
            leaguePlayers = leagueSchedules.SelectMany(l => l.ScheduledGames)
                                                             .Where(s => s.IsComplete && !s.WasCancelled)
                                                             .Select(s => s.GameResults)
                                                             .SelectMany(g => g.Teams)
                                                             .SelectMany(t => t.Players)
                                                             .GroupBy(p => p.Name)
                                                             .Select(gp => new
                                                             {
                                                                 gp.Key,
                                                                 Player = new PlayerStats(GetSummaryData(gp.ToList()), gp.ToList().Count)
                                                             }).OrderByDescending(p => p.Player.NumGames)
                                                               .ThenByDescending(p => p.Player.AtBats)
                                                               .ThenBy(p => p.Player.Name)
                                                               .Select(p => p.Player);
            PlayerStats summaryStats = new(GetSummaryData(leaguePlayers, summaryName))
            {
                NumGames = numGames
            };
            leaguePlayers = leaguePlayers.Append(summaryStats);
            return leaguePlayers;
        }

        public static Player GetSummaryData(IEnumerable<Player> playerData, string summaryName = "")
        {
            Player player = Player.Empty;
            if ((playerData != null) && playerData.Any())
            {
                string nameText = !string.IsNullOrEmpty(summaryName) ? summaryName : playerData.First().Name;
                player = Player.ConstructPlayer(new List<PlayerLabelValue> { new("Player", nameText) });
                player = playerData.SumIntProperties<Player>(player);
            }

            return player;
        }

        //public static T SumIntProperties<T>(IEnumerable<T> data, T instance)
        //{
        //    IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
        //                                                    .Where(p => p.PropertyType == typeof(int));
        //    foreach (PropertyInfo property in properties)
        //    {
        //        property.SetValue(instance, data.Select(p => (int)property.GetValue(p)).ToList().Sum());
        //    }

        //    return instance;
        //}


        //public IEnumerable<LeagueSchedule> GetLeagueSchedules(LeagueDescription? leagueDescription = null)
        //{
        //    IEnumerable<LeagueSchedule> leagueSchedules = DataStore.LeagueSchedules;

        //    if (leagueDescription != null)
        //    {

        //        leagueSchedules = GetLeagueSchedules(leagueDescription.LeagueCategory,
        //                                             leagueDescription.LeagueDay,
        //                                             leagueDescription.Season,
        //                                             leagueDescription.Year);
        //    }

        //    return leagueSchedules;
        //}

        /// <summary>
        /// Returns a sequence of <see cref="LeagueSchedule"/> objects filtered by the values of the parameters
        /// </summary>
        /// <remarks>
        /// All parameters are optional, and if not specified the league schedules are not filtered by the value of the 
        /// corresponding <see cref="LeagueDescription"/> property. If no parameters are specified, then all league schedules
        /// are returned. The filtering is in the same order as the position of the parameters. For example,
        /// <code language="cs">
        /// GetLeagueSchedules("Community", season: "Fall")
        /// </code> 
        /// returns league schedules for all Community leagues for the Fall season.
        /// <para>
        /// If any of the parameters are invalid (for example, specifying "Wednesday" for the 2023 Fall Competitive league), 
        /// no league schedules are returned. The following tables illustrates the valid values from the Fall 20223 leagues.
        /// </para>
        /// <list type="table">
        /// <listheader >
        /// <term>Category</term>
        /// <description>Days</description>
        /// <item>
        /// <term>Recreation</term><description>Monday</description>
        /// </item>
        /// <item>
        /// <term>Community</term><description>Monday, Tuesday, Wednesday, Friday</description>
        /// </item>
        /// <item>
        /// <term>Competitive</term><description>Tuesday, Friday</description>
        /// </item>
        /// <item>
        /// <term>Sidewinder</term><description>Wednesday</description>
        /// </item>
        /// <item>
        /// <term>Coyote</term><description>Thursday</description>
        /// </item>
        /// </listheader>
        /// </list>
        /// </remarks>
        /// <param name="category">
        /// The league category and the default value is the empty string (no filtering done on the league category).
        /// </param>
        /// <param name="day">
        /// The day and the default value is the empty string (no filtering done on the day).
        /// </param>
        /// <param name="season">
        /// The season and the default value is the empty string (no filtering done on the season).
        /// </param>
        /// <param name="year">
        /// The year and the default value is the empty string (no filtering done on the league category).
        /// </param>
        /// <returns>The filtered sequence of <see cref="LeagueSchedule"/> objects.</returns>
        public IEnumerable<LeagueSchedule> GetLeagueSchedules(string category = "", string day = "", string season = "", string year = "")
        {
            Dictionary<string, List<string>> validLeagues = ValidLeagueDescriptions();
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

        // TODO: This should be static and only executed once.
        public Dictionary<string, List<string>> ValidLeagueDescriptions()
        {
            Dictionary<string, List<string>> validLeagues = new();
            foreach (LeagueDescription description in LeagueDescriptions)
            {
                string category = description.LeagueCategory;
                string day = description.LeagueDay;
                if (!validLeagues.TryGetValue(category, out List<string>? value))
                {
                    value = new List<string>();
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

        public static IEnumerable<LeagueSchedule> GetSchedules(IEnumerable<LeagueSchedule> schedules, string propertyName, string propertyValue)
        {
            IEnumerable<LeagueSchedule> newSchedules = schedules ?? Enumerable.Empty<LeagueSchedule>();
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

    }
}
