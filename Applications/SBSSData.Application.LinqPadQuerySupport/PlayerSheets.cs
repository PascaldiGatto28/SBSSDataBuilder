// Ignore Spelling: Linq Css

using System.Reflection;

using HtmlAgilityPack;

using LINQPad;

using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using Query = SBSSData.Softball.Stats.Query;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class PlayerSheets : IHtmlCreator
    {
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "Player summary league data for all players and leagues"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["Player's League Summaries", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        public PlayerSheets()
        {
            Values = [];
            HtmlContainerName = "PlayerSheetsContainer.html";
        }

        public PlayerSheets(string containerName) : this()
        {
            HtmlContainerName = containerName;
        }
        

        public List<object> Values
        {
            get;
            set;
        }

        public string HtmlContainerName
        {
            get;
            set;
        }


        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            Action<object> actionCallback = callback == null ? (v) => Console.WriteLine(v.ToString()) : callback;
            string season = seasonText.RemoveWhiteSpace();
            string playerPhotos = "../PlayerPhotos/";

            string changedHtml = string.Empty;

            Assembly assembly = typeof(PlayerSheets).Assembly;

            // Not creating a title for this version -- that comes directly from the container HTML file
            //string resName = assembly.FormatResourceName("PlayerSheets.html");
            //byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            //string introHtml = bytes.ByteArrayToString();

            string resName = assembly.FormatResourceName(HtmlContainerName);
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string containerHtml = bytes.ByteArrayToString();
            containerHtml = containerHtml.Replace("[[Season YYYY]]", Utilities.SwapSeasonText(seasonText));

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

                // Just in case the map doesn't have everyone in it, for each player name, if they're not in the map, put
                // them in.

                foreach (string player in playerNames)
                {
                    if (!map.ContainsKey(player))
                    {
                        map.Add(player, "Available_Photo-Not");
                    }
                }

                List<string> optionValues = playerNames.Select(p => 
                                                       $"""
                                                        <div 
                                                            class="playerOption" playerName="{p}" 
                                                            imageName="{playerPhotos}{map[p]}.jpg">
                                                            {p.BuildDisplayName()}
                                                        </div>
                                                        """).ToList();

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
                        IEnumerable<Player> playerData = leagueGames.SelectMany(g => g.Teams)
                                                                    .SelectMany(t => t.Players)
                                                                    .Where(p => p.Name == playerName);

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
                            LeagueTotals = leagueTotals.PlayersSummary($"{shortLeagueName}"),
                            PlayerPercentiles = GetRankPercentile(playerName, league, query)
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
                        generator.WriteRootTable(pscd.SheetItems.Select(s => s.Totals), ExtendedPlayerSheets(headerStyle, psc));
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

                    HtmlNode viewAllNode = playersList.SelectSingleNode(".//div");
                    viewAllNode.InnerHtml = $"View All {optionValues.Count} Players";
                    changedHtml = root.OuterHtml;

                    actionCallback($"{this.GetType().Name} HTML page created.");
                }

            }

            return changedHtml;
        }

        public List<PlayerSheetPercentile> GetRankPercentile(string playerName, LeagueName league, Query query)
        {
            List<PlayerSheetPercentile>? piList = [];
            List<PlayerStats> ps = query.GetLeaguePlayers(league.Category, league.Day)
                                        .Where(p => p.PlateAppearances > 5).ToList();
            int n = ps.Count();
            List<string> propertyNames = ["Average","Slugging","OnBase", "OnBasePlusSlugging"];
            List<PlayerSheetPercentile> playersInfo = [];
            foreach (string propertyName in propertyNames)
            {
                PropertyInfo? property = typeof(PlayerStats).GetProperty(propertyName);
                if (property != null)
                {
                    List<PlayerStats> propertyNameStats = ps.OrderBy(p => property.GetValue(p)).ThenBy(p => p.PlateAppearances).ToList();
                    for (int i = 0; i < n; i++)
                    {
                        int percentile = (int)Math.Round(((double)((i == 0) ? 0 : i) / (double)n) * 100, 0);
                        int rank = n - i;
                        PlayerSheetPercentile pInfo = new PlayerSheetPercentile(n, propertyNameStats[i].Name, propertyName, (double)(property.GetValue(propertyNameStats[i]) ?? 0), rank, percentile);
                        playersInfo.Add(pInfo);
                    }
                }
            }

            IEnumerable<IGrouping<string, PlayerSheetPercentile>> groups = playersInfo.GroupBy(i => i.PlayerName);//.Dump("PlayerSheetPercentils Groups");
            IEnumerable<PlayerSheetPercentile>? group = groups.SingleOrDefault(g => g.Key == playerName);

            if ((group != null) && group.Any())
            {
                piList = group.ToList();
            }

            return piList;

        }

        public static Func<TableNode, string> ExtendedPlayerSheets(string? headerCssStyle, PlayerSheetContainer playerSheetContainer)
        {
            return (t) =>
            {
                HtmlNode tableHtmlNode = t.TableHtmlNode;
                string header = t.Header().InnerText;
                string introductionHeader = $"""
                                                <div style="font-weight:500; font-family:'Segoe UI Semibold'; font-size:14px; 
                                                color:white; background-color:#d62929; 
                                                border-radius:15px; 
                                                padding:7px; margin:-8px 10px 5px 15px; width:640px; text-align:center;">
                                                   {playerSheetContainer.Introduction}
                                                </div>
                                                """;

                if (t.Depth() == 0)
                {
                    // Add an identifier (player name) to the table.
                    tableHtmlNode.Attributes.Add("playerName", playerSheetContainer.PlayerSheetItems.First().PlayerName);

                    // Add background color to the containing cell to get a contrast with the text that is placed in the anchor
                    // tag.
                    HtmlNode td = tableHtmlNode.SelectSingleNode("./thead/tr/td");
                    string headerStyle = td.GetAttributeValue("style", null);
                    headerStyle = string.IsNullOrEmpty(headerStyle) ? "background-color:#a6bad9;" : $""""background-color:#a6bad9; {headerStyle}"""";
                    td.Attributes.Add("style", headerStyle);

                    header = introductionHeader; // playerSheetContainer.Introduction;
                }
                else if (t.Depth() == 1)
                {
                    header = playerSheetContainer.PlayerSheetItems[t.Index()].Description;
                    Utilities.UpdatePlayerColumnNames(tableHtmlNode);
                    tableHtmlNode.SelectSingleNode("./thead/tr[2]/th[1]").Remove();
                    tableHtmlNode.SelectNodes("./tbody/tr/td[1]").ToList().ForEach(n => n.Remove());

                    string ranksTR = """
                                    <tr style="background-color:#e8efff">
                                    <td colspan="12" class="n";>This is the Ranks </td>
                                    <td class="n" title="[[playerName]]'s position in the list of all players AVG values">A</td>
                                    <td class="n" title="[[playerName]]'s position in the list of all players SLG values">B</td>
                                    <td class="n" title="[[playerName]]'s position in the list of all players OBP values">C</td>
                                    <td class="n" title="[[playerName]]'s position in the list of all players OPS values">D</td>
                                    </tr>
                                    """;
                    string percentilesTR = """
                                            <tr style="background-color:#e8efff">
                                            <td colspan="12" class="n"; style="text-align:left;">This is the Percentiles </td>
                                            <td class="n" title="The percentage of all players having an AVG value less than [[playerName]]'s">A</td>
                                            <td class="n" title="The percentage of all players having an SLG value less than [[playerName]]'s">B</td>
                                            <td class="n" title="The percentage of all players having an OBP value less than [[playerName]]'s">C</td>
                                            <td class="n" title="The percentage of all players having an OPS value less than [[playerName]]'s">D</td>
                                            </tr>
                                            """;

                    List<PlayerSheetItem> psItems = playerSheetContainer.PlayerSheetItems;
                    string league = tableHtmlNode.SelectSingleNode("./tbody/tr[2]/td[1]").InnerText;
                    for (int i = 0; i < psItems.Count; i++)
                    {
                        PlayerSheetItem psItem = psItems[i];
                        string playerName = psItem.PlayerName;
                        string firstName = playerName.Substring(playerName.IndexOf(' ') + 1);
                        if (psItem.PlayerPercentiles.Any() && (league == psItem.LeagueName))
                        {
                            string ranks = ranksTR.Replace("[[playerName]]", firstName);
                            HtmlNode ranksRow = HtmlNode.CreateNode(ranks);
                            HtmlNode firstRow = tableHtmlNode.SelectSingleNode("./tbody/tr");
                            tableHtmlNode.SelectSingleNode("./tbody").InsertAfter(ranksRow, firstRow);

                            string percentiles = percentilesTR.Replace("[[playerName]]", firstName);
                            HtmlNode percentilesRow = HtmlNode.CreateNode(percentiles);
                            tableHtmlNode.SelectSingleNode("./tbody").InsertAfter(percentilesRow, firstRow);

                            List<HtmlNode> percentTds = percentilesRow.SelectNodes(".//td").ToList();
                            List<HtmlNode> rankTds = ranksRow.SelectNodes(".//td").ToList();
                            for (int j = 1; j < percentTds.Count; j++)
                            {
                                PlayerSheetPercentile percentile = psItem.PlayerPercentiles[j - 1];
                                percentTds[j].InnerHtml = percentile.PercentileToString();
                                rankTds[j].InnerHtml = percentile.Rank.ToString();
                            }

                            percentTds[0].InnerHtml = $"{psItem.PlayerPercentiles[0].NumPlayers} players having more than 5 plate appearance used for calculations. <span style='float:right'>Percentiles =></span>";
                            rankTds[0].InnerHtml = $"Rankings =>";

                        }
                    }

                    Utilities.UpdatePlayerHeaderTitles(tableHtmlNode, false);


                }

                //tableHtmlNode.SetAttributeValue("style", "display:none");

                return header; // + $" Depth = {t.Depth()} Index = {t.Index()}";
            };
        }
    }
}
