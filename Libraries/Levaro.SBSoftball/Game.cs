using HtmlAgilityPack;

using Levaro.SBSoftball.Common;

using Newtonsoft.Json;

namespace Levaro.SBSoftball
{
    /// <summary>
    /// The class has just properties, but together hold all the stats for every game in every league. Moreover
    /// the <see cref="GameInformation"/> is really just meta-data that is created when the data store class
    /// <see cref="LeaguesData"/> is created. It never needs to change. All the game data in terms of stats resides in the
    /// <see cref="Teams"/> property that is updated during the course of a season as games are played.
    /// </summary>
    public class Game
    {
        /// <summary>
        /// Creates an "empty" instance of the class.
        /// </summary>
        /// <remarks>
        /// Because this constructor is private and both property settings are "init", the only recommended way to create an
        /// instance of the class us using the <see cref="ConstructGame"/> static method.
        /// </remarks>
        private Game()
        {
            Teams = Enumerable.Empty<Team>().ToList();
        }

        /// <summary>
        /// Gets and initializes the <c>GameInformation</c> property which encapsulates all the meta-data for the scheduled
        /// game.
        /// </summary>
        /// <remarks>
        /// This property is only initialized once when the <see cref="Game"/> instance is constructed by the 
        /// <see cref="ConstructGame"/> method and should not be altered after that.
        /// </remarks>
        public GameInformation? GameInformation
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the <see cref="Team"/> classes (visitors and home) for a scheduled game.
        /// </summary>
        /// <remarks>
        /// Again, the <see cref="ConstructGame(ScheduledGame, bool)"/> method that initializes this property. If a game that has not played or is
        /// cancelled, it is initialized to an empty list. If the <see cref="Game"/> class is updated or initialized when a
        /// game is completed, each of the elements of the list (visitor and home teams) do include all the team and player stats
        /// for that game.
        /// </remarks>
        public List<Team> Teams
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the <see cref="HtmlAgilityPack"/> <see cref="HtmlDocument"/> for the HTML page have all game information.
        /// </summary>
        /// <remarks>
        /// The <see cref="ConstructGame(ScheduledGame, bool)"/> static method parses the page using the 
        /// <see cref="ScheduledGame.ResultsUrl"/>. This private method is used to make the Web site is available for recovering
        /// data.
        /// </remarks>
        /// <param name="scheduledGame">The scheduled game whose <see cref="ScheduledGame.ResultsUrl"/> url is to recover the
        /// HTML page.
        /// </param>
        /// <returns>The <see cref="HtmlDocument"/> for the page"/> if both the <paramref name="scheduledGame"/> 
        /// and its <c>ResultsUrl</c> property are not <c>null</c> and the document does not contain an <see cref="HtmlNode"/>
        /// having a class named "Maintenance". Although exceptions can be thrown, if method succeeds, the returned
        /// document is never <c>null</c>, but could be empty.
        /// </returns>
        /// <exception cref="InvalidOperationException">This occurs when the "maintenance" class is found in the document,
        /// which means the Web site is current not available because the data is being updated.</exception>
        /// <exception cref="ArgumentNullException">if either the <paramref name="scheduledGame"/> or its <c>ResultsUrl</c>
        /// is null.</exception>
        private static HtmlDocument? GetHtmlDocument(ScheduledGame scheduledGame)
        {
            HtmlDocument? htmlDocument;
            if ((scheduledGame != null) && (scheduledGame.ResultsUrl != null))
            {
                htmlDocument = PageContentUtilities.GetPageHtmlDocument(scheduledGame.ResultsUrl);
                if (htmlDocument.DocumentNode.SelectSingleNode("//body").HasClass("maintenance"))
                {
                    throw new InvalidOperationException("The data is not available, web site data is currently being updated.");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(scheduledGame), "Scheduled game or the scheduled game ResultsUrl are null.");
            }

            return htmlDocument;
        }

        /// <summary>
        /// Constructs a new <see cref="Game"/> instance or updates an existing one.
        /// </summary>
        /// <param name="scheduledGame">Initializes or updates the <see cref="ScheduledGame.GameResults"/> property for this
        /// scheduled game. 
        /// </param>
        /// <param name="update">If <c>true</c>, the <see cref="Teams"/> property is updated with team and player stats if 
        /// the game is completed but not cancelled. Otherwise, the <see cref="GameInformation"/> and <c>Teams</c> properties
        /// are both initialized, the if the game is completed and not cancelled, the stats information is also recorded.
        /// </param>
        /// <returns>A new <see cref="Game"/> instance; <c>null</c> is never returned</returns>
        public static Game ConstructGame(ScheduledGame scheduledGame, bool update = false)
        {
            // Actually GetHtmlDocument can never return null, because it checks for cases if scheduledGame is null.
            // GetHtmlDocument uses PageContentUtilities.GetPageHtmlDocument, and it returns
            HtmlDocument? htmlDocument = Game.GetHtmlDocument(scheduledGame) ??
                                         throw new NullReferenceException("The HtmlDocument cannot be null when construction a Game instance");

            GameInformation gameInformation = new GameInformation();
            if (!update)
            {
                if ((scheduledGame.GameResults == null) || string.IsNullOrEmpty(scheduledGame.GameResults.GameInformation?.Title))
                {
                    gameInformation = GameInformation.ConstructGameInformation(scheduledGame, htmlDocument);
                }
            }
            else
            {
                // GameResults should never be null because if this is an update, GameResults is already constructed.
                gameInformation = scheduledGame.GameResults.GameInformation;
            }

            // If this not an update, and because the game has been played, but not cancelled, then we need to recover the Teams
            // list, and if it is an update, we always to recover the Teams data.
            List<Team> teams = new();
            if (!update)
            {
                if ((scheduledGame.IsComplete) && (!scheduledGame.WasCancelled))
                {
                    teams = Game.ConstructTeams(htmlDocument);
                }
            }
            else
            {
                teams = Game.ConstructTeams(htmlDocument);
            }

            return new Game()
            {
                GameInformation = gameInformation,
                Teams = teams
            };
        }

        /// <summary>
        /// Returns <c>true</c> if the game is complete; <c>false otherwise</c>
        /// </summary>
        /// <remarks>
        /// Notice that both this class and <see cref="ScheduledGame"/> have the same (non-serialized) property, but the
        /// implementation is different. For <see cref="ScheduledGame.IsComplete"/> is true if and only if both the
        /// visiting and home scores have values. Here it <c>true</c> if only if the <see cref="Teams"/> is not the empty
        /// list. The reason for the difference is twofold: (1) want to use just the properties from the class instance,
        /// and (2) more importantly to be able to recognize cancelled games. When a game is cancelled, <c>Teams</c> is
        /// empty, but because the score for cancelled games is 0 for both visiting and home, the scheduled game is
        /// considered completed so it won't be checked again for team data.
        /// </remarks>
        /// <returns><c>true</c> if the <c>Teams</c> property is neither <c>null</c> nor empty.</returns>
        [JsonIgnore]
        public bool IsCompleted => (Teams != null) && Teams.Any();

        /// <summary>
        /// Construct the <see cref="GameInformation"/> instance for the <see cref="Game.GameInformation"/> property.
        /// </summary>
        /// <remarks>
        /// This method is only called from this class and the only time it is called is when the class is instantiated and
        /// not when updated. Because it shares the same HtmlDocument that the <see cref="ConstructGame(ScheduledGame, bool)"/>
        /// static method uses, it the <c>GameInformation</c> is only created from the <c>Game</c> class this static here
        /// rather than the <c>GameInformation</c> class.
        /// </remarks>
        /// <param name="scheduledGame">
        /// The <see cref="ScheduledGame"/> for which the <see cref="ScheduledGame.GameResults"/> property (that is, <c>Game</c>
        /// object) is created.
        /// </param>
        /// <param name="htmlDocument">The <see cref="HtmlDocument"/> for the page having the game data</param>
        /// <returns>A <c>GameInformation</c> instance; never <c>null</c>.</returns>
        private static GameInformation ConstructGameInformation(ScheduledGame scheduledGame, HtmlDocument htmlDocument)
        {
            string title = htmlDocument.DocumentNode.SelectSingleNode("//article/header/h1").InnerHtml.CleanNameText();
            Uri? dataSource = scheduledGame.ResultsUrl;
            string gameId = string.Empty;
            if (dataSource != null)
            {
                string lastSegment = dataSource.Segments.ToList().Last();
                string id = lastSegment;
                if (lastSegment.EndsWith("/"))
                {
                    int length = lastSegment.Length - 1;
                    id = lastSegment[..length];
                }

                gameId = id;
            }

            DateTime date = scheduledGame.Date;

            List<HtmlNode> spSectionContentNodes = htmlDocument.DocumentNode.SelectSingleNode("//article/div").SelectNodes("div").ToList();
            HtmlNode gameDetails = spSectionContentNodes.Where(n => n.HasClass("sp-section-content-details")).Single();
            List<string> details = gameDetails.SelectSingleNode("//tbody/tr").Elements("td").Select(n => n.InnerHtml).ToList();
            string[] league = details[2].Split(' ');
            string leagueCategory = league[1].Trim();
            string leagueDay = league[0].Trim();
            string season = details[3].Split(' ')[0].Trim();

            GameInformation gameInformation = new()
            {
                Title = title,
                GameId = gameId,
                DataSource = dataSource,
                Date = date,
                LeagueCategory = leagueCategory,
                LeagueDay = leagueDay,
                Season = season,
            };

            return gameInformation;
        }

        /// <summary>
        /// Constructs the list of <see cref="Team"/> objects that contain team game information, and team/player stats.
        /// </summary>
        /// <remarks>
        /// This method called when both updating or creating the <c>Game</c> object using 
        /// <see cref="ConstructGame(ScheduledGame, bool)"/>. This is the only location where a <c>Team</c> instance is created.
        /// When the sequence of <see cref="ScheduledGame"/> objects are created, the <c>Teams</c> property may be changed:
        /// <list type="bullet">
        ///     <item>
        ///     <code language="c#">
        ///         If the game has not been played, the team list is empty (not <c>null</c>, that is
        ///         List{Team} Teams = Enumerable.Empty{Team}().ToList();
        ///     </code>
        ///     and this method is not called.
        ///     </item>
        ///     <item>
        ///     If the game is complete, the <c>Team</c> instances are created , including the <see cref="Player"/> sequence,
        ///     having all team information and stats. The first team in the list is the visitors, the second the home team.
        ///     </item>
        ///     <item>
        ///     If the game is not complete but has been recorded in the Web site, that means it has been cancelled. In this
        ///     case this method is not called, but instead the scores in the <c>ScheduledGame</c> object are changed to 0 to 
        ///     indicate there is no need to check for changes.
        ///     </item>
        /// </list>
        /// After all the scheduled games have been initialized (and the results persisted &mdash; JSON serialized), changes
        /// in games played means the scheduled games are checked to see if the results have been recorded. If so, this
        /// method is called and returns the updated list of <c>Teams</c>
        /// </remarks>
        /// <param name="htmlDocument">The HTML document segment having the team and player information</param>
        /// <returns>Returns a list of <c>Team</c> objects. It may be empty, but never <c>null</c>.</returns>
        private static List<Team> ConstructTeams(HtmlDocument htmlDocument)
        {
            List<HtmlNode> spSectionContentNodes = htmlDocument.DocumentNode.SelectSingleNode("//article/div").SelectNodes("div").ToList();

            // Now each of the teams. The second section provide summary information about the team for this game.
            List<Team> teams = new();
            HtmlNode sectionContentResults = spSectionContentNodes.Where(n => n.HasClass("sp-section-content-results")).Single();
            if ((sectionContentResults != null) && sectionContentResults.HasChildNodes)
            {


                List<HtmlNode> resultNodes = sectionContentResults.SelectNodes("div/div/table/tbody/tr")
                                                                  .Where(n => n.NodeType == HtmlNodeType.Element)
                                                                  .ToList();


                bool homeTeam = false;
                foreach (HtmlNode resultNode in resultNodes)
                {
                    string[] results = resultNode.SelectNodes("td")
                                                 .Where(n => n.NodeType == HtmlNodeType.Element)
                                                 .Select(n => n.InnerHtml)
                                                 .ToArray();
                    Team team = new()
                    {
                        Name = results[0].CleanNameText(),
                        RunsScored = results[1].ParseInt() ?? 0,
                        Hits = results[2].ParseInt() ?? 0,
                        HomeTeam = homeTeam  // The first team is the visiting team
                    };

                    homeTeam = !homeTeam;
                    team.Outcome = results[3];

                    teams.Add(team);
                }

                // The RunsAgainst are the runs scored for the other team. This is needed when stats for a team are aggregated.
                for (int i = 0; i < 2; i++)
                {
                    teams[i].RunsAgainst = teams[(i + 1) % 2].RunsScored;
                }

                // The third section provides the name for each player and their stats

                HtmlNode sectionContentPerformanceTeams = spSectionContentNodes.Where(n => n.HasClass("sp-section-content-performance"))
                                                                               .Single()
                                                                               .Element("div");
                IEnumerable<HtmlNode> sectionContentPerformanceValues = sectionContentPerformanceTeams.Elements("div");

                // Each node in the this sectionContentPerformanceValues represents the players of that team
                foreach (HtmlNode htmlNode in sectionContentPerformanceValues)
                {
                    string teamName = htmlNode.Element("h4").InnerHtml.CleanNameText();
                    List<Player> players = new();
                    List<HtmlNode> rawTeamStats = htmlNode.SelectNodes("div/table/tbody/tr")
                                                          .Where(n => n.NodeType == HtmlNodeType.Element)
                                                          .ToList();

                    // Each row provides the data for each player in the team.
                    foreach (HtmlNode teamStatsNode in rawTeamStats)
                    {
                        var teamStats = teamStatsNode.SelectNodes("td")
                                                     .Where(n => n.NodeType == HtmlNodeType.Element)
                                                     .Select(n => new
                                                     {
                                                         DataLabel = n.Attributes.Where(a => a.Name == "data-label").Single().Value,
                                                         Value = n.InnerText
                                                     })
                                                     .ToList();
                        Player player = new();
                        foreach (var stats in teamStats)
                        {
                            switch (stats.DataLabel)
                            {
                                case "Player":
                                {
                                    player.Name = stats.Value.CleanNameText();
                                    break;
                                }
                                case "AB":
                                {
                                    player.AtBats = int.Parse(stats.Value);
                                    break;
                                }
                                case "R":
                                {
                                    player.Runs = int.Parse(stats.Value);
                                    break;
                                }
                                case "1B":
                                {
                                    player.Singles = int.Parse(stats.Value);
                                    break;
                                }
                                case "2B":
                                {
                                    player.Doubles = int.Parse(stats.Value);
                                    break;
                                }
                                case "3B":
                                {
                                    player.Triples = int.Parse(stats.Value);
                                    break;
                                }
                                case "HR":
                                {
                                    player.HomeRuns = int.Parse(stats.Value);
                                    break;
                                }
                                case "BB":
                                {
                                    if (int.TryParse(stats.Value, out int result))
                                    {
                                        player.BasesOnBalls = int.Parse(stats.Value);
                                    }
                                    else
                                    {
                                        player.BasesOnBalls = 0;
                                    }
                                    break;
                                }
                                case "SF":
                                {
                                    player.SacrificeFlies = int.Parse(stats.Value);
                                    break;
                                }
                                default:
                                {
                                    break;
                                }
                            }
                        }

                        players.Add(player);
                    }

                    Team summary = teams.Single(s => s.Name == teamName);
                    summary.Players = players;
                }
            }

            return teams;
        }
    }
}
