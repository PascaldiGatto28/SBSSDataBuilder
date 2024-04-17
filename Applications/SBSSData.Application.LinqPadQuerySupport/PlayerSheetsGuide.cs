// Ignore Spelling: Linq

using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class PlayerSheetsGuide
    {
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "A guide to the Player Sheets page"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["name", "http-equiv"], ["content", "no-cache"]]),
            new HeadElement("title", [["Player League Summaries", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "SBSSData.ico"]])
        };

        public PlayerSheetsGuide()
        {
            Values = [];
        }

        public List<object> Values
        {
            get;
            set;
        }

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            Action<object> actionCallback = callback == null ? (v) => Console.WriteLine(v.ToString()) : callback;
            string season = seasonText.RemoveWhiteSpace();
            string playerPhotos = "PlayerPhotos/";

            string changedHtml = string.Empty;

            Assembly assembly = typeof(PlayerSheets).Assembly;

            //string resName = assembly.FormatResourceName("PlayerSheets.html");
            //byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            //string introHtml = bytes.ByteArrayToString();

            string resName = assembly.FormatResourceName("PlayerSheetsContainerGuide.html");
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string containerHtml = bytes.ByteArrayToString();

            string headerStyle = "background-color:#d62929; width:680px";
            string path = $"{dataStoreFolder}{season}LeaguesData.json";

            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(path))
            {
                Query query = new(dsContainer);

                List<PlayerSheetContainer> playerSheetContainers = [];

                IEnumerable<string> playerNames = query.GetActivePlayerNames();

                // First change the containerHtml to include the HTML that is the list of selection player options. Later I replace
                // the iFrame source doc attribute to the HTML of the returned HTML.
                Dictionary<string, string> map = PlayerPhotos.GetPlayerName2ImageNameMap();
                List<string> optionValues = playerNames.Select(p => $"""<div class="playerOption" playerName="{p}" imageName="{playerPhotos}{map[p]}.jpg">{p.BuildDisplayName()}</div>""").ToList();

                foreach (string playerName in playerNames)
                {
                    IEnumerable<LeagueName> leagueNames = query.GetLeagueNamesForPlayer(playerName);
                    int totalTeams = 0;
                    int totalGames = 0;
                    int numTeams = 0;
                    int numGames = 0;

                    List<PlayerSheetItem> sheetData = [];
                    foreach (LeagueName league in leagueNames)
                    {
                        PlayerSheetItem playerSheetItem = new();

                        IEnumerable<Game> leagueGames = query.GetLeaguePlayedGames(league.Category, league.Day);
                        IEnumerable<Player> playerData = leagueGames.SelectMany(g => g.Teams).SelectMany(t => t.Players).Where(p => p.Name == playerName);

                        IEnumerable<Team> teams = leagueGames.SelectMany(g => g.Teams.Where(t => t.Players.Select(p => p.Name).Contains(playerName)));
                        numTeams = query.GetTeamsPlayersStats(league.Category, league.Day).SelectMany(ts => ts.Players).Where(p => p.Name == playerName).Count();

                        IEnumerable<Player> playersTotals = leagueGames.SelectMany(g => g.Teams).SelectMany(t => t.Players).Where(p => p.Name == playerName);
                        numGames = playersTotals.Count();
                        IEnumerable<Player> leagueTotals = query.GetTeamsPlayersStats(league.Category, league.Day).Select(ts => ts.Players).Select(l => l.Last());

                        totalTeams += numTeams;
                        totalGames += numGames;

                        string shortLeagueName = league.ShortLeagueName;
                        playerSheetItem = new()
                        {
                            PlayerName = playerName,
                            LeagueName = shortLeagueName,
                            NumGames = numGames,
                            NumTeams = numTeams,
                            LeagueNumGames = totalGames,
                            LeagueNumTeams = totalTeams,
                            PlayerTotals = playersTotals.PlayersSummary(),
                            LeagueTotals = leagueTotals.PlayersSummary($"{shortLeagueName}")
                        };

                        sheetData.Add(playerSheetItem);
                    }

                    string introduction = $"""
                                           {playerName.BuildDisplayName()} played in {leagueNames.Count().NumDesc("league")} 
                                           for {totalTeams.NumDesc("team")} in {totalGames.NumDesc("game")} during the 
                                           {query.GetSeason()} season
                                           """;

                    //sheetData.SelectMany(s => s.PlayerStats
                    PlayerSheetContainer psc = new PlayerSheetContainer(introduction, sheetData);
                    playerSheetContainers.Add(psc);
                }

                using (HtmlGenerator generator = new HtmlGenerator())
                {
                    // Using the results from the queries to produce tables, write the tables and any
                    // other data to the XhtmlWriter (via HtmlGenerator.Write methods).

                    foreach (PlayerSheetContainer psc in playerSheetContainers)
                    {

                        IEnumerable<PlayerSheetItemDisplay> psd = psc.PlayerSheetItems.Select(si => new PlayerSheetItemDisplay(si));
                        PlayerSheetContainerDisplay pscd = new PlayerSheetContainerDisplay(psc);
                        //List<List<PlayerStatsDisplay>> sheetStats = pscd.SheetItems.Select(s => s.Totals).ToList();
                        generator.WriteRootTable(pscd.SheetItems.Select(s => s.Totals), LinqPadCallbacks.ExtendedPlayerSheets(headerStyle, psc));
                    }


                    //string htmlNode = introHtml.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    //HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    changedHtml = generator.DumpHtml(//pageTitle: title,
                                                     cssStyles: StaticConstants.LocalStyles + StaticConstants.PlayerSheetsStyles,
                                                     javaScript: StaticConstants.LocalJavascript,
                                                     collapseTo: 2,
                                                     headElements: headElements.ToList());

                    // Now populate the PlayrSheetsContainer HTML document by inserting the player list and changing the
                    // scrdoc attribute on iframe element. So first load the file Html in a HtmlDocument.
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc = PageContentUtilities.GetHtmlDocument(containerHtml);
                    HtmlNode root = htmlDoc.DocumentNode;
                    HtmlNode sheets = root.SelectSingleNode("//iframe[@id='sheets']");
                    sheets.Attributes["srcdoc"].Remove();
                    sheets.Attributes.Add("srcdoc", changedHtml);
                    HtmlNode playersList = root.SelectSingleNode("//div[@id='playersList']");
                    foreach (string playerOption in optionValues)
                    {
                        playersList.AppendChild(HtmlNode.CreateNode(playerOption));
                    }

                    changedHtml = root.OuterHtml;
                }

            }

            return changedHtml;
        }
    }
}
