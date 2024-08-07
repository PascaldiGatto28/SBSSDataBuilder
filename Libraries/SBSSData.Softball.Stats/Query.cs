﻿using System.Reflection;

using SBSSData.Softball.Common;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// TODO: Queries should allow a leagues to be specified like league players method.
    /// </summary>
    public class Query
    {
        private static Dictionary<string, List<string>> validLeaguesDictionary = [];

        public static readonly List<string> ComputedStatNames = ["Average", "Slugging", "OnBase", "OnBasePlusSlugging"];

        public static readonly List<string> ComputedStatDisplayNames = ["AVG", "SLG", "OBP", "OPS"];

        private Query()
        {
            Container = DataStoreContainer.Empty;
        }

        public Query(DataStoreContainer dsContainer)
        {
            Container = dsContainer ?? DataStoreContainer.Empty;
            validLeaguesDictionary = this.ValidLeagueDescriptions();
        }

        public static Query Empty => new();

        public DataStoreContainer Container
        {
            get;
            set;
        }

        public LeaguesData DataStore => Container.DataStore;


        public IEnumerable<LeagueDescription> GetLeagueDescriptions() => DataStore.LeagueSchedules.Select(s => s.LeagueDescription);

        //public IEnumerable<LeagueName> GetLeagueNames() => GetLeagueDescriptions().OrderBy(d => d.LeagueCategory).Select(l => new LeagueName(l)).ToList();

        public IEnumerable<LeagueName> GetLeagueNames(IEnumerable<LeagueDescription>? leagueDescription = null)
        {

            IEnumerable<LeagueName> leagueNames = [];
            IEnumerable<LeagueDescription> descriptions = leagueDescription ?? GetLeagueDescriptions();
            return descriptions.OrderBy(d => d.LeagueCategory).Select(l => new LeagueName(l)).ToList();
        }

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
        /// Get league names for every league for which a player has played in a game in that league
        /// </summary>
        /// <param name="playerName">The player name must in the format "lastName, FirstName"</param>
        /// <returns>A list of <see cref="LeagueName"/> objects.</returns>
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

        public string GetSeason()
        {
            LeagueDescription league = GetLeagueDescriptions().First();
            return $"{league.Season} {league.Year}";
        }

        public IEnumerable<ScheduledGame> ScheduledGames => Container.GetScheduledGames();


        public IEnumerable<Game> GetPlayedGames() => ScheduledGames.Where(s => s.IsComplete && !s.WasCanceled)
                                                                   .Select(s => s.GameResults)
                                                                   .Where(s => !s.IsForfeited);

        public IEnumerable<Game> GetLeaguePlayedGames(string leagueCategory = "", string day = "")
        {
            return GetLeagueSchedules(leagueCategory, day).SelectMany(l => l.ScheduledGames)
                                                          .Where(s => s.IsComplete && !s.WasCanceled && !s.GameResults.IsForfeited)
                                                          .Select(s => s.GameResults);


        }

        public IEnumerable<Player> GetActivePlayers() => GetPlayedGames().SelectMany(g => g.Teams)
                                                                         .SelectMany(t => t.Players)
                                                                         .OrderBy(p => p.Name).ToList();
        public IEnumerable<string> GetActivePlayerNames() =>  GetActivePlayers().Select(p => p.Name).Distinct().OrderBy(p => p);

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

        public IEnumerable<PlayerStats> PlayersStats => GetActivePlayers().Select(p => new PlayerStats(p));

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

        public TeamSummaryStats GetTeamPlayersStats(string teamName, string leagueCategory, string day)
        {
            // TODO: Do error checking here.
            IEnumerable<TeamSummaryStats> teamsSummaryStats = GetTeamsPlayersStats(leagueCategory, day);
            TeamSummaryStats teamSummaryStats = teamsSummaryStats.Single(t => t.Name == teamName);
            return teamSummaryStats;
        }



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

        public override string ToString()
        {
            return Container.ToString();
        }
    }
}
