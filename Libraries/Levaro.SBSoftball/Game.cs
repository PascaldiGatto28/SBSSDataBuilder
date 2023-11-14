using HtmlAgilityPack;

using Levaro.SBSoftball.Common;

using Newtonsoft.Json;

namespace Levaro.SBSoftball
{
    public class Game
    {
        public Game()
        {
            GameInformation = new GameInformation();
            Teams = Enumerable.Empty<Team>().ToList();
        }

        public GameInformation GameInformation
        {
            get; set;
        }

        public List<Team> Teams
        {
            get;
            set;
        }


        //public Game(Uri dataSource) : this()
        //{
        //    HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(dataSource);
        //    Initialize(htmlDocument);
        //    GameInformation.DataSource = dataSource;
        //    string lastSegment = dataSource.Segments.ToList().Last();
        //    string id = lastSegment;
        //    if (lastSegment.EndsWith("/"))
        //    {
        //        int length = lastSegment.Length - 1;
        //        id = lastSegment[..length];
        //    }

        //    GameInformation.GameId = id;
        //}

        //public static Game InitGame(ScheduledGame scheduledGame)
        //{
        //    Game game = new Game();
        //    if ((scheduledGame != null) && (scheduledGame.ResultsUrl != null))
        //    {
        //        HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(scheduledGame.ResultsUrl);
        //        if (htmlDocument.DocumentNode.SelectSingleNode("//body").HasClass("maintenance"))
        //        {
        //            throw new InvalidOperationException("The data is not available, web site data is currently being updated.");
        //        }

        //        GameInformation gameInformation = game.GameInformation;
        //        if (string.IsNullOrEmpty(scheduledGame.GameResults.GameInformation.Title))
        //        {
        //            gameInformation = game.ConstructGameInformation(scheduledGame, htmlDocument);
        //        }

        //        List<Team> teams = new List<Team>();
        //        if ((scheduledGame.IsComplete) && (!scheduledGame.WasCancelled))
        //        {
        //            teams = game.ConstructTeams(htmlDocument);
        //        }

        //        game = new Game()
        //        {
        //            GameInformation = gameInformation,
        //            Teams = teams
        //        };
        //    }

        //    return game;
        //}

        //public static Game UpdateGame(ScheduledGame scheduledGame)
        //{

        //    Game game = new Game();
        //    if ((scheduledGame != null) && (scheduledGame.ResultsUrl != null))
        //    {
        //        game = scheduledGame.GameResults;
        //        HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(scheduledGame.ResultsUrl);
        //        if (htmlDocument.DocumentNode.SelectSingleNode("//body").HasClass("maintenance"))
        //        {
        //            throw new InvalidOperationException("The data is not available, web site data is currently being updated.");
        //        }

        //        //if (string.IsNullOrEmpty(game.GameInformation.Title))
        //        //{
        //        //    scheduledGame.GameResults.GameInformation = game.ConstructGameInformation(scheduledGame, htmlDocument);
        //        //}

        //        if ((!scheduledGame.IsComplete) && (!scheduledGame.WasCancelled))
        //        {
        //            scheduledGame.GameResults.Teams = game.ConstructTeams(htmlDocument);
        //        }

        //    }

        //    return game;
        //}

        private static HtmlDocument? GetHtmlDocument(ScheduledGame scheduledGame)
        {
            HtmlDocument? htmlDocument = null;
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
            HtmlDocument? htmlDocument = Game.GetHtmlDocument(scheduledGame);
            if (htmlDocument == null)
            {
                throw new NullReferenceException("The HtmlDocument cannot be null when construction a Game instance");
            }

            Game game = update ? scheduledGame.GameResults : new Game();
            GameInformation gameInformation = new GameInformation();
            if (!update)
            {
                if (string.IsNullOrEmpty(scheduledGame.GameResults.GameInformation.Title))
                {
                    gameInformation = game.ConstructGameInformation(scheduledGame, htmlDocument);
                }
            }
            else
            {
                gameInformation = game.GameInformation;
            }

            List<Team> teams = new List<Team>();
            if ((scheduledGame.IsComplete) && (!scheduledGame.WasCancelled))
            {
                teams = game.ConstructTeams(htmlDocument);
            }

            game = new Game()
            {
                GameInformation = gameInformation,
                Teams = teams
            };


            return game;
        }

        //public Game(string dataSource) : this()
        //{
        //    HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(dataSource);
        //    Initialize(htmlDocument);

        //}

        //private void Initialize(HtmlDocument htmlDocument)
        //{
        //    if (htmlDocument.DocumentNode.SelectSingleNode("//body").HasClass("maintenance"))
        //    {
        //        throw new InvalidOperationException("The data is not available, web site data is currently being updated.");
        //    }

        //    ConstructTeams(htmlDocument);
        //}

        [JsonIgnore]
        public bool IsCompleted => (Teams != null) && Teams.Any();

        private GameInformation ConstructGameInformation(ScheduledGame scheduledGame, HtmlDocument htmlDocument)
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

            GameInformation gameInformation = new GameInformation()
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


            //HtmlNode linkNode = htmlDocument.DocumentNode.SelectSingleNode("/html/head/link[@rel='canonical']");
            //if (linkNode != null)
            //{
            //    info.DataSource = new(linkNode.GetAttributeValue("href", null));

            //    string lastSegment = info.DataSource.Segments.ToList().Last();
            //    string id = lastSegment;
            //    if (lastSegment.EndsWith("/"))
            //    {
            //        int length = lastSegment.Length - 1;
            //        id = lastSegment[..length];
            //    }

            //    info.GameId = id;
            //}
            //else
            //{
            //    info.DataSource = null;
            //    info.GameId = string.Empty;
            //}

            //info.Title = htmlDocument.DocumentNode.SelectSingleNode("//article/header/h1").InnerHtml.CleanNameText();
            //List<HtmlNode> spSectionContentNodes = htmlDocument.DocumentNode.SelectSingleNode("//article/div").SelectNodes("div").ToList();

            //HtmlNode gameDetails = spSectionContentNodes.Where(n => n.HasClass("sp-section-content-details")).Single();
            //List<string> details = gameDetails.SelectSingleNode("//tbody/tr").Elements("td").Select(n => n.InnerHtml).ToList();

            //if (DateTime.TryParse($"{details[0]} {details[1]}", out DateTime date))
            //{
            //    info.Date = date;
            //}
            //else
            //{
            //    info.Date = DateTime.MinValue;
            //}

            //string[] league = details[2].Split(' ');
            //info.LeagueCategory = league[1].Trim();
            //info.LeagueDay = league[0].Trim();
            //info.Season = details[3].Split(' ')[0].Trim();

            //return info;
        }

        private List<Team> ConstructTeams(HtmlDocument htmlDocument)
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
