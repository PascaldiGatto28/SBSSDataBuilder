using HtmlAgilityPack;

using Newtonsoft.Json;

using SBSSData.Softball.Common;

namespace SBSSData.Softball
{
    /// <summary>
    /// The class has two properties, but together hold all the stats for every game in every league.
    /// </summary>
    /// <remarks>
    /// The <see cref="GameInformation"/> is really just meta-data that is created when the data store class
    /// <see cref="LeaguesData"/> is created. It never needs to change. All the game data in terms of stats resides in the
    /// <see cref="Teams"/> property that is updated during the course of a season as games are played.
    /// </remarks>
    public sealed class Game
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
            GameInformation = GameInformation.Empty;
        }

        /// <summary>
        /// Gets and initializes the <c>GameInformation</c> property which encapsulates all the meta-data for the scheduled
        /// game.
        /// </summary>
        /// <remarks>
        /// This property is only initialized once when the <see cref="Game"/> instance is constructed by the 
        /// <see cref="ConstructGame"/> method and should not be altered after that.
        /// </remarks>
        public GameInformation GameInformation
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the <see cref="Team"/> classes (visitors and home) for a scheduled game.
        /// </summary>
        /// <remarks>
        /// The <see cref="ConstructGame(ScheduledGame, bool)"/> method initializes this property.If a game has not been played 
        /// or is cancelled, it is initialized to an empty list. If the <see cref="Game"/> class is updated or initialized when a
        /// game is completed, each of the elements of the list (visitor and home teams) include all the team and player stats
        /// for that game.
        /// </remarks>
        public List<Team> Teams
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the <see cref="HtmlDocument"/> for the HTML page that has all game information.
        /// </summary>
        /// <remarks>
        /// The <see cref="ConstructGame(ScheduledGame, bool)"/> static method parses the page using the 
        /// <paramref name="scheduledGame"/> <see cref="ScheduledGame.ResultsUrl"/> property value. 
        /// This private method is used to make sure the Web site is available for recovering data.
        /// </remarks>
        /// <param name="scheduledGame">The scheduled game whose <see cref="ScheduledGame.ResultsUrl"/> property value is used to 
        /// create a <see cref="Uri"/> to recover the HTML page.
        /// </param>
        /// <returns>The <see cref="HtmlDocument"/> for the page if both the <paramref name="scheduledGame"/> 
        /// and its <c>ResultsUrl</c> property are not <c>null</c> and the document does not contain an <see cref="HtmlNode"/>
        /// having a class named "maintenance". Although exceptions are not thrown, the returned
        /// HTMLDocument is never <c>null</c>, but could be empty.
        /// </returns>
        /// <exception cref="InvalidOperationException">This occurs when the "maintenance" class is found in the document,
        /// which means the Web site is current not available because the data is being updated.</exception>
        /// <exception cref="ArgumentNullException">if either the <paramref name="scheduledGame"/> or its <c>ResultsUrl</c>
        /// is null.</exception>
        private static HtmlDocument GetHtmlDocument(ScheduledGame scheduledGame)
        {
            HtmlDocument htmlDocument;
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
        /// are both initialized, then if the game is completed and not cancelled, the stats information is also recorded.
        /// </param>
        /// <returns>A new <see cref="Game"/> instance; <c>null</c> is never returned</returns>
        public static Game ConstructGame(ScheduledGame scheduledGame, bool update = false)
        {
            // Actually GetHtmlDocument can never return null, because it checks for cases if scheduledGame is null.
            // GetHtmlDocument uses PageContentUtilities.GetPageHtmlDocument, and it returns
            HtmlDocument? htmlDocument = Game.GetHtmlDocument(scheduledGame) ??
                                         throw new NullReferenceException("The HtmlDocument cannot be null when constructing a Game instance");

            GameInformation gameInformation = GameInformation.Empty;
            List<Team> teams = new();
            if (scheduledGame != null)
            {
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
                    gameInformation = scheduledGame.GameResults?.GameInformation ?? GameInformation.Empty;
                }

                // If this not an update, and because the game has been played, but not cancelled, then we need to recover the Teams
                // list, and if it is an update, we always need to recover the Teams data.

                if (!update)
                {
                    if (scheduledGame.IsComplete && (!scheduledGame.WasCancelled))
                    {
                        teams = Game.ConstructTeams(htmlDocument);
                    }
                }
                else
                {
                    teams = Game.ConstructTeams(htmlDocument);
                }
            }

            return new Game()
            {
                GameInformation = gameInformation,
                Teams = teams
            };
        }

        /// <summary>
        /// Returns an "empty" game object, that is, one with properties set to default values.
        /// </summary>
        /// <remarks>
        /// An empty game is just used as an initialized <see cref="Game"/> object and because the properties can only be
        /// initialized during construction, it has no use otherwise.
        /// </remarks>
        /// <seealso cref="ConstructGame(ScheduledGame, bool)"/>
        [JsonIgnore]
        public static Game Empty => new();

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

        ///// <remarks>
        ///// <para>
        ///// This method is called when both updating or creating the <c>Game</c> object using the 
        ///// <see cref="ConstructGame(ScheduledGame, bool)"/> static method, which is the only way a <c>Team</c> 
        ///// instance is created.
        ///// </para>
        ///// <para>
        ///// When the sequence of <see cref="ScheduledGame"/> objects are created, the <c>Teams</c> property may be changed as well.
        ///// After all the scheduled games have been initialized (and the results persisted &mdash; JSON serialized), changes
        ///// in games played means the scheduled games are checked to see if the results have been recorded. If so, this
        ///// method is called and returns the updated list of <c>Teams</c>
        ///// </para>
        ///// </remarks>
        /// <summary>
        /// Constructs the list of objects that contain <see cref="Team"/> game information and team/player stats.
        /// </summary>
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
                        HomeTeam = homeTeam,  // The first team is the visiting team
                        Outcome = results[3]
                    };

                    homeTeam = !homeTeam;
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

                    // Each row provides the data for each player that participated in this team.
                    foreach (HtmlNode teamStatsNode in rawTeamStats)
                    {
                        IEnumerable<PlayerLabelValue> labelValues = teamStatsNode.SelectNodes("td")
                                                     .Where(n => n.NodeType == HtmlNodeType.Element)
                                                     .Select(n => new
                                                     {
                                                         DataLabel = n.Attributes.Where(a => a.Name == "data-label").Single().Value,
                                                         Value = n.InnerText
                                                     })
                                                     .ToList().Select(t => new PlayerLabelValue(t.DataLabel, t.Value));

                        Player player = Player.ConstructPlayer(labelValues);
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
