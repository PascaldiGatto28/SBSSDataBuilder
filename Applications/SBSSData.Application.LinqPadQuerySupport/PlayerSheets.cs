// Ignore Spelling: Linq Css

using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using Query = SBSSData.Softball.Stats.Query;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class PlayerSheets : IHtmlCreator
    {
        private static readonly HeadElement[] headElements =
        [
            new("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new("meta", [["data", "description"], ["content", "Player summary league data for all players and leagues"]]),
            new("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new("title", [["Player's League Summaries", ""]]),
            new("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        ];

        public PlayerSheets()
        {
            Values = [];
            HtmlContainerName = "PlayerSheetsContainer.html";
            IntermediateFilePath = string.Empty;
        }

        public PlayerSheets(string containerName, string intermediateFilePath = "") 
        {
            HtmlContainerName = containerName;
            IntermediateFilePath = intermediateFilePath;
            Values = [];
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

        public string IntermediateFilePath
        {
            get; 
            set; 
        }


        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            Action<object>? actionCallback = callback; // == null ? (v) => Console.WriteLine(v.ToString()) : callback;
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
            List<string> optionValues = [];

            List<PlayerSheetContainer> playerSheetContainers = GetPlayerSheetContainers(path, playerPhotos, ref optionValues).ToList();
            using (HtmlGenerator generator = new())
            {
                // Using the results from the queries to produce tables, write the tables and any
                // other data to the XhtmlWriter (via HtmlGenerator.Write methods).

                foreach (PlayerSheetContainer psc in playerSheetContainers)
                {

                    IEnumerable<PlayerSheetItemDisplay> psd = psc.PlayerSheetItems.Select(si => new PlayerSheetItemDisplay(si));
                    PlayerSheetContainerDisplay pscd = new(psc);
                    generator.WriteRootTable(pscd.SheetItems.Select(s => s.Totals), ExtendedPlayerSheets(headerStyle, psc));
                }


                //string htmlNode = introHtml.Substring("<div class=\"IntroContent\"", "</body", true, false);
                //HtmlNode title = HtmlNode.CreateNode(htmlNode);
                changedHtml = generator.DumpHtml(//pageTitle: title,
                                                    cssStyles: StaticConstants.LocalStyles + StaticConstants.PlayerSheetsStyles,
                                                    javaScript: StaticConstants.LocalJavascript,
                                                    collapseTo: 2,
                                                    headElements: headElements.ToList());

                if (!string.IsNullOrEmpty(IntermediateFilePath))
                {
                    try
                    {
                        File.WriteAllText(IntermediateFilePath, changedHtml);
                        actionCallback($"{IntermediateFilePath} created and contains the changedHtml from PlayerSheets.BuildHtml");
                    }
                    catch (Exception exception)
                    {
                        throw new InvalidOperationException($"The {IntermediateFilePath} is invalid", exception);
                    }
                }
                else
                {

                    // Now populate the PlayrSheetsContainer HTML document by inserting the player list and changing the
                    // scrdoc attribute on iframe element. So first load the file Html in a HtmlDocument.
                    HtmlDocument htmlDoc = new();
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
                    actionCallback?.Invoke($"{this.GetType().Name} HTML page created.");
                }
            }


            return changedHtml;
        }

        public static Dictionary<string, LeaguesAndPlayersStatistics> GetLeaguesAndPlayersStatistics(Query query)
        {
            Dictionary<string, IEnumerable<LeagueStatistics>> ls = query.GetLeaguesStatistics()     
                                                                        .ToDictionary(k => k.Key, k => k.Value.Select(v => new LeagueStatistics(v)));
                 

            IEnumerable<LeagueName> leagueNames = query.GetLeagueNames();
            Dictionary<string, LeaguesAndPlayersStatistics> allLeagues = [];
            foreach (LeagueName leagueName in leagueNames)
            {
                var groups = query.GetAllPlayersStatistics(leagueName, 5);
                allLeagues.Add(leagueName.ShortLeagueName, new LeaguesAndPlayersStatistics(ls[leagueName.ShortLeagueName], groups));
            }

            return allLeagues;
        }
        
        public static List<PlayerSheetPercentile> GetRankPercentile(string playerName, 
                                                                    LeagueName leagueName, 
                                                                     Dictionary<string, LeaguesAndPlayersStatistics> allLeagues)
        {
            List<PlayerSheetPercentile> percentiles = [];
            LeaguesAndPlayersStatistics lps = allLeagues[leagueName.ShortLeagueName];
            if (lps.PlayerStatistics.TryGetValue(playerName, out IEnumerable<PlayerSheetPercentile>? ptiles))
            {
                percentiles = ptiles?.ToList() ?? [];
            }

            return percentiles;
        }

        public static IEnumerable<PlayerSheetContainer> GetPlayerSheetContainers(string dsPath, string playerPhotosPath, ref List<string> optionValues)
        {
            List<PlayerSheetContainer> playerSheetContainers = [];
            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(dsPath))
            {
                Query query = new(dsContainer);
                IEnumerable<string> playerNames = query.GetActivePlayerNames();

                // First change the containerHtml to include the HTML that is the list of selection player options. Later I replace
                // the iFrame source doc attribute to the HTML of the returned HTML.
                Dictionary<string, string> map = PlayerPhotos.GetPlayerName2ImageNameMap();

                // Just in case the map doesn't have everyone in it, for each player name, if they're not in the map, put
                // them in.

                foreach (string player in playerNames)
                {
                    map.TryAdd(player, "Available_Photo-Not");
                }

                optionValues = playerNames.Select(p =>
                                               $"""
                                                <div 
                                                    class="playerOption" playerName="{p}" 
                                                    imageName="{playerPhotosPath}{map[p]}.jpg">
                                                    {p.BuildDisplayName()}
                                                </div>
                                                """).ToList();

                Dictionary<string, LeaguesAndPlayersStatistics> leaguesAndPlayersStatistics = PlayerSheets.GetLeaguesAndPlayersStatistics(query);
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
                            PlayerPercentiles = PlayerSheets.GetRankPercentile(playerName, league, leaguesAndPlayersStatistics),
                            LeagueStatistics = leaguesAndPlayersStatistics[shortLeagueName].LeagueStatistics.ToList()
                        };

                        sheetData.Add(playerSheetItem);
                    }

                    string introduction = $"""
                                   {playerName.BuildDisplayName()} played in {leagueNames.Count().NumDesc("league")} 
                                   for {totalTeams.NumDesc("team")} in {totalGames.NumDesc("game")} during the 
                                   {query.GetSeason()} season
                                   """;

                    PlayerSheetContainer psc = new(introduction, sheetData);
                    playerSheetContainers.Add(psc);
                }
            }

            return playerSheetContainers;
        }

        public static Func<TableNode, string> ExtendedPlayerSheets(string? headerCssStyle, PlayerSheetContainer playerSheetContainer)
        {
            return (t) =>
            {
                _ = headerCssStyle ?? "Not specified";
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
                    string zScoresTR = """
                                        <tr style="background-color:#e8efff">
                                        <td colspan="12" class="n"; style="text-align:left;">This is the ZScores </td>
                                        <td class="n" title="[[playerName]]'s [[value]] AVG is [[zscore]] StdDevs [[AboveBelow]] the league mean, which is [[Mean]] for AVG">A</td>
                                        <td class="n" title="[[playerName]]'s [[value]] SLG is [[zscore]] StdDevs [[AboveBelow]] the league mean, which is [[Mean]] for SLG">B</td>
                                        <td class="n" title="[[playerName]]'s [[value]] OBP is [[zscore]] StdDevs [[AboveBelow]] the league mean, which is [[Mean]] for OPB">C</td>
                                        <td class="n" title="[[playerName]]'s [[value]] OPS is [[zscore]] StdDevs [[AboveBelow]] the league mean, which is [[Mean]] for OPS">D</td>
                                        </tr>
                                        """;

                    List<PlayerSheetItem> psItems = playerSheetContainer.PlayerSheetItems;
                    string league = tableHtmlNode.SelectSingleNode("./tbody/tr[2]/td[1]").InnerText;
                    for (int i = 0; i < psItems.Count; i++)
                    {
                        PlayerSheetItem psItem = psItems[i];
                        string playerName = psItem.PlayerName;
                        string firstName = playerName[(playerName.IndexOf(' ') + 1)..];
                        if ((psItem.PlayerPercentiles.Count != 0) && (league == psItem.LeagueName))
                        {
                            List<LeagueStatistics> leagueStatistics = psItem.LeagueStatistics;
                            
                            string zScores = zScoresTR.Replace("[[playerName]]", firstName);
                            HtmlNode zScoresRow = HtmlNode.CreateNode(zScores);

                            List<HtmlNode> zTdWithTitle = zScoresRow.SelectNodes(".//td[@title]").ToList();

                            HtmlNode firstRow = tableHtmlNode.SelectSingleNode("./tbody/tr");
                            tableHtmlNode.SelectSingleNode("./tbody").InsertAfter(zScoresRow, firstRow);

                            string ranks = ranksTR.Replace("[[playerName]]", firstName);
                            HtmlNode ranksRow = HtmlNode.CreateNode(ranks);
                            tableHtmlNode.SelectSingleNode("./tbody").InsertAfter(ranksRow, firstRow);

                            string percentiles = percentilesTR.Replace("[[playerName]]", firstName);
                            HtmlNode percentilesRow = HtmlNode.CreateNode(percentiles);
                            tableHtmlNode.SelectSingleNode("./tbody").InsertAfter(percentilesRow, firstRow);

                            List<HtmlNode> percentTds = percentilesRow.SelectNodes(".//td").ToList();
                            List<HtmlNode> rankTds = ranksRow.SelectNodes(".//td").ToList();
                            List<HtmlNode> zScoreTds = zScoresRow.SelectNodes(".//td").ToList();
                            for (int j = 1; j < percentTds.Count; j++)
                            {
                                PlayerSheetPercentile percentile = psItem.PlayerPercentiles[j - 1];
                                percentTds[j].InnerHtml = percentile.PercentileToString();

                                double zScore = percentile.ZScore;
                                string prefix = zScore > 0 ? "+" : "-";
                                string zScoreAbsValue = Math.Abs(zScore).ToString("0.00");
                                zScoreTds[j].InnerHtml = $"{prefix}{zScoreAbsValue}";

                                // Fix up the titles
                                string zScoreTdTitle = zScoreTds[j].GetAttributeValue("title", null);
                                zScoreTdTitle = zScoreTdTitle.Replace("[[value]]", $"{percentile.PropertyValue:#.000}")
                                                             .Replace("[[zscore]]", zScoreAbsValue);
                                string aboveBelow = (prefix == "+") ? "above" : "below";
                                zScoreTdTitle = zScoreTdTitle.Replace("[[AboveBelow]]", aboveBelow)
                                                             .Replace("[[Mean]]", $"{leagueStatistics[j - 1].Mean:#.000}");
                                zScoreTds[j].SetAttributeValue("title", zScoreTdTitle);


                                rankTds[j].InnerHtml = percentile.Rank.ToString();
                            }

                            percentTds[0].InnerHtml = $"{psItem.PlayerPercentiles[0].NumPlayers} players having more than 5 plate appearance used for calculations. <span style='float:right'>Percentiles =></span>";
                            rankTds[0].InnerHtml = $"Rankings =>";
                            zScoreTds[0].InnerHtml = $"<span style='float:right'>Z-scores =></span>";

                            
                            HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
                            List<HtmlNode> leagueTds = lastRow.SelectNodes(".//td").ToList();
                            List<string> sdName = Query.ComputedStatDisplayNames;
                            for (int j = 0; j < sdName.Count; j++) 
                            {
                                leagueTds[12+j].SetAttributeValue("title", $"League mean for {sdName[j]}={leagueStatistics[j].Mean:#.000} and StdDev={leagueStatistics[j].StdDev:0.00}");
                            }
                        }
                    }

                    Utilities.UpdatePlayerHeaderTitles(tableHtmlNode, false);
                }

                return header; 
            };
        }
    }
}
