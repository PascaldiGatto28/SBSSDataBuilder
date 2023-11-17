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
        /// Again, the <see cref="ConstructGame"/> method that initializes this property. If a game that has not played or is
        /// cancelled, it is initialized to an empty list. If the <see cref="Game"/> class is updated or initialized when a
        /// game is completed, each of the elements of the list (visitor and home teams) do include all the team and player stats
        /// for that game.
        /// </remarks>
        public List<Team> Teams
        {
            get;
            init;
        }

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

        public static Game ConstructGame(ScheduledGame scheduledGame, bool update = false)
        {
            HtmlDocument? htmlDocument = Game.GetHtmlDocument(scheduledGame) ??
                                         throw new NullReferenceException("The HtmlDocument cannot be null when construction a Game instance");

            // If this is an update, that means the scheduled already has GameResults (that is, a GameInformation instance)
            Game game; // = update ? scheduledGame.GameResults : null;
            GameInformation gameInformation = new();
            if (!update)
            {
                if ((scheduledGame.GameResults == null) || string.IsNullOrEmpty(scheduledGame.GameResults.GameInformation.Title))
                {
                    gameInformation = Game.ConstructGameInformation(scheduledGame, htmlDocument);
                }
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
                GameInformation = !update ? gameInformation : scheduledGame.GameResults.GameInformation,
                Teams = teams
            };
        }

        [JsonIgnore]
        public bool IsCompleted => (Teams != null) && Teams.Any();

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

        private static List<Team> ConstructTeams(HtmlDocument htmlDocument)
        {
            // See the Sample Game Source.html file in the Documents folder of this project of document source that is
            // scraped to recover the game information.

            //IEnumerable<HtmlNode> spSectionContentNodes = ConstructGameInformation(htmlDocument);
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
