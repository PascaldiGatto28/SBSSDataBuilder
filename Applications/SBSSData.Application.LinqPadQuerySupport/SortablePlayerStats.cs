using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using Spectre.Console;

using Query = SBSSData.Softball.Stats.Query;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class SortablePlayerStats : IHtmlCreator
    {
        private static HeadElement[] headElements =
        [
            new("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new("meta", [["data", "description"], ["content", "Player league stats for all players and leagues using sortable tables"]]),
            new("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new("title", [["Player's League Summaries", ""]]),
            new("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        ];

        public SortablePlayerStats()
        {
            Values = [];
            ResourceName = $"{this.GetType().Name}.html";
        }

        public SortablePlayerStats(string resourceName, string intermediateFilePath = "")
        {
            ResourceName = resourceName;
            IntermediateFilePath = intermediateFilePath;
            Values = [];
        }

        public List<object> Values
        {
            get;
            set;
        }

        public string ResourceName
        {
            get;
            set;
        }
        

        public string IntermediateFilePath
        {
            get;
            set;
        }
        public static HeadElement[] HeadElements
        {
            get => headElements;
            set => headElements = value;
        }

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            Action<object>? actionCallback = callback; // == null ? (v) => Console.WriteLine(v.ToString()) : callback;
            string season = seasonText.RemoveWhiteSpace();

            string changedHtml = string.Empty;

            Assembly assembly = typeof(SortablePlayerStats).Assembly;
            string resName = assembly.FormatResourceName(ResourceName);
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();
            html = html.Replace("[[Season YYYY]]", Utilities.SwapSeasonText(seasonText));

            string path = $"{dataStoreFolder}{season}LeaguesData.json";

            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(path))
            {
                Query query = new(dsContainer);

#pragma warning disable IDE0063 // Use simple 'using' statement
                using (HtmlGenerator generator = new())
                {
                    string expandCollapseHtml = """
                                                    <div>
                                                         <button class="sbss" title="Click to expand all tables" onclick = "viewAll(true)">Expand All Tables</button>
                                                         <button class="sbss" title="Click to collapse all tables to a single line" onclick = "viewAll(false)">Collapse All Tables</button>
                                                         <button class="sbss" title="Click to toggle the visibility of league and player statistics" onclick = "toggleStatisticsDisplay(this)">Display Statistics</button>
                                                    </div> 
                                                    """;
                    string headerCss = """
                                        background-color:#4c74b2;
                                        font-weight:500;
                                        color: white;
                                        font-size:1.20em;
                                        """;

                    
                    IEnumerable<LeagueName> leagueNames = query.GetLeagueNames();
                    List<PropertyInfo> properties = typeof(PlayerStats).GetProperties()
                                                                       .Where(p => Query.ComputedStatNames.Contains(p.Name))
                                                                       .ToList();

                    List<LeaguePlayersStatistics> leaguePlayersStatistics = [];
                    foreach (LeagueName league in leagueNames)
                    {
                        LeaguePlayersStatistics? lss = GetLeaguePlayersStatistics(query, league.Category, seasonText, properties, league.Day);
                        if ((lss != null) && lss.PlayersStatistics.Count() > 1)
                        {
                            leaguePlayersStatistics.Add(lss);
                        }
                    }

                    if (leaguePlayersStatistics.Count == 0)
                    {
                        string noData = """<div style="font-size:18px; color:Firebrick; font-weight:500;">No data is available yet, because no games have been played!</div>""";
                        generator.WriteRawHtml(noData);
                    }
                    else
                    {
                        generator.WriteRawHtml(expandCollapseHtml);
                        generator.WriteRootTable(leaguePlayersStatistics, ExtendedSortablePlayerStats(headerCss, leaguePlayersStatistics));

                        leaguePlayersStatistics = [];
                        IEnumerable<string> categoryNames = leagueNames.GroupBy(l => l.Category).Where(g => g.Count() > 1).Select(g => g.Key);
                        foreach (string category in categoryNames)
                        {
                            LeaguePlayersStatistics? lss = GetLeaguePlayersStatistics(query, category, seasonText, properties);
                            if (lss != null)
                            {
                                leaguePlayersStatistics.Add(lss);
                            }
                        }

                        generator.WriteRootTable(leaguePlayersStatistics, ExtendedSortablePlayerStats(headerCss, leaguePlayersStatistics));
                    }

                    string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    html = generator.DumpHtml(pageTitle: title,
                                              cssStyles: StaticConstants.LocalStyles +
                                                         StaticConstants.HelpStyles +
                                                         StaticConstants.SortableTableStyles,
                                              javaScript:StaticConstants.LocalJavascript + StaticConstants.HelpJavascript,
                                              collapseTo: 1,
                                              headElements: [.. headElements]);

                    callback?.Invoke("Sortable Player Stats constructed");
                }
#pragma warning restore IDE0063 // Use simple 'using' statement
            }

            return html;
        }

        // For a league (or league category), return LeaguePlayersStatistic object having the league name
        // the all player stats including z-Scores for the league sorting by plate appearances in descending
        // order and the league descriptive statistics (the weighted (PA) mean and std deviation used for the
        // calculation of each of the z-Scores for each active player.
        // query parameter is based upon the season and associate path of the Leagues JSON file and returned from 
        // the data store container.
        // category is the league name category, e.g. "Community"
        // seasonText is the season name, for example""2024 Spring"
        // properties is the list of PropertyInfo for the PlayerStats class
        // day the name of day for the league; if left blank all leagues of the same category are used
        public static LeaguePlayersStatistics? GetLeaguePlayersStatistics(Query query,
                                                                          string category,
                                                                          string seasonText,
                                                                          List<PropertyInfo> computedProperties,
                                                                          string day = "")
        {
            LeaguePlayersStatistics? lps = null;
            string season = Utilities.SwapSeasonText(seasonText);
            LeagueName league = new(day, category, $"{day} {category} {season}", $"{day} {category}");

            // Get all the players stats. Note this gets all players (not just ones have a qualified number of
            // plate appearances) and then sorted in descending order.
            List<PlayerStats> playersStats = [.. query.GetLeaguePlayersSummary(league.Category, league.Day)
                                                      .OrderByDescending(p => p.PlateAppearances)];
            PlayerStats summary = playersStats[0];
            if (playersStats.Count != 0)
            {
                // First the league weighted descriptive statistics are found using the plate appearances as weights
                // the playersStats as the weights. Have to remove the summary item when calculating league(s) statistics.

                playersStats.RemoveAt(0);
                IEnumerable<double> weights = playersStats.Select(ps => (double)ps.PlateAppearances);

                // The computed properties (Average, Slugging, OnBase and OnBasePlusSlugging) are used to retrieve the
                // values for each player to compute the Z-scores.
                List<StatisticsDisplay> dsList = [];
                for (int i = 0; i < computedProperties.Count; i++)
                {
                    PropertyInfo property = computedProperties[i];
                    IEnumerable<double> values = playersStats.Select(ps => (double)(property.GetValue(ps) ?? double.NaN));
                    string statName = property.Name;
                    StatisticsDisplay sd = new(values.GetStatistics(statName.NameToTitle(), weights));
                    dsList.Add(sd);
                }

                // Now that we have the league mean and standard deviation, we can compute the Z-score for each of the
                // computed player stats for each player.
                List<PlayerStatistics> playersStatisticsDisplay = [];
                playersStats.Add(summary);
                foreach (PlayerStats ps in playersStats)
                {
                    List<double> zScores = [];
                    for (int i = 0; i < dsList.Count; i++)
                    {
                        StatisticsDisplay sd = dsList[i];
                        double mean = sd.Mean;
                        double stdDev = sd.StdDev;
                        double value = (double?)computedProperties[i]?.GetValue(ps) ?? double.NaN;
                        double zScore = (value - mean) / stdDev;
                        zScores.Add(zScore);
                    }

                    // Finally use the z-Scores to create a PlayerStatistics record from the the player stats
                    // object and array of z-Scores, and add the record to the list of records.
                    PlayerStatistics pStatistics = new(ps, zScores);
                    playersStatisticsDisplay.Add(pStatistics);
                }

                // Put the pieces together and return a LeaguePlayersStatistics object/
                lps = new LeaguePlayersStatistics(league, playersStatisticsDisplay, dsList);
            }

            return lps;
        }

        public static Func<TableNode, string> ExtendedSortablePlayerStats(string? headerCssStyle = null, object? value = null)
        {
            return (t) =>
            {
                IEnumerable<LeaguePlayersStatistics> leaguePlayersStatistics = (IEnumerable<LeaguePlayersStatistics>?)value ?? [];
                bool isSummaryTables = string.IsNullOrEmpty(leaguePlayersStatistics.First().League.Day);
                HtmlNode tableNode = t.TableHtmlNode;
                string headerTableInfo = $"Depth / Index = {t.Depth()}/{t.Index()}";
                string headerText = "Sortable Tables";

                // Create the sortable player summary stats table
                int listIndex = (t.Index() - 1) / 3;
                int index = t.Index() % 3;
                if (t.Depth() == 1)
                {
                    switch (index)
                    {
                        case 1:
                        {
                            tableNode.AddClass("sortable");
                            List<HtmlNode> headerNodes = [.. tableNode.SelectNodes(".//th")];
                            List<int> zScoreHeaderIndexes = [];


                            List<int> stringElementList = [];
                            for (int i = 0; i < headerNodes.Count; i++)
                            {
                                HtmlNode headerNode = headerNodes[i];
                                if (headerNode.GetAttributeValue("title", null) == "System.String")
                                {
                                    stringElementList.Add(i);
                                }

                                if (headerNode.InnerHtml.Contains("ZScore"))
                                {
                                    headerNode.AddClass("hidden");
                                    headerNode.AddClass("zScore");
                                    zScoreHeaderIndexes.Add(i);
                                    headerNode.InnerHtml = headerNode.InnerHtml.Replace("ZScore", " Z-score");
                                }
                            }

                            // Now for each zScore, add the class "hidden" to it, a the class zScore and typeData attribute.
                            var rows = tableNode.SelectNodes("./tbody//tr").ToList();
                            foreach (HtmlNode row in rows)
                            {
                                // Skip the first 16 cells (indexes 0 to 15) and start from the 17th cell (index 16)
                                int startColumn = zScoreHeaderIndexes.First();
                                int endColumn = zScoreHeaderIndexes.Last();
                                foreach (HtmlNode cell in row.SelectNodes($"td[position() > {startColumn} and position() <= {endColumn + 1}]"))
                                {
                                    cell.AddClass("zScore");
                                    cell.AddClass("hidden");
                                    if (!double.TryParse(cell.InnerText, out double zScore))
                                    {
                                        zScore = 0;
                                    }

                                    
                                    cell.Attributes.Add("zScoreData", zScore.ToString());
                                    string zText = $"{Math.Round(zScore, 2):+0.00;-0.00; 0.00}";
                                    cell.InnerHtml = zText;
                                }

                                
                            }

                            string stringElements = $"""'[{stringElementList.ToString<int>(", ")}]'""";
                            string tableSelector = $"'#{t.Id()}'";

                            Utilities.UpdatePlayerColumnNames(tableNode);
                            Utilities.UpdatePlayerHeaderTitles(tableNode, true, false, true);

                            // Testing removing first data row to the footer

                            HtmlNode tbody = tableNode.SelectSingleNode("./tbody");
                            HtmlNode tfoot = tableNode.SelectSingleNode("./tfoot");
                            if (tfoot != null)
                            {
                                tableNode.RemoveChild(tfoot);
                            }

                            tfoot = HtmlNode.CreateNode("<tfoot><span></span></tfoot>");
                            tableNode.AppendChild(tfoot);

                            HtmlNode lastRow = tbody.SelectSingleNode("tr[last()]");
                            if (lastRow != null)
                            {
                                // Remove the first row from tbody
                                tbody.RemoveChild(lastRow);
                                
                                
                                // Append the first row to tfoot
                                tfoot.FirstChild.AppendChild(HtmlNode.CreateNode(lastRow.OuterHtml));
                            }

                            // TODO: The columnIndex is the initial column that is sorted in descending order. This information
                            // should not be hard-coded.
                            string eventScript = StaticConstants.SortableTable
                                                                .Replace("[[columnIndex]]", "3")
                                                                .Replace("[[tableSelector]]", tableSelector)
                                                                .Replace("[[stringElements]]", stringElements);
                            HtmlNode script = HtmlNode.CreateNode(eventScript);
                            tableNode.ParentNode.InsertAfter(script, tableNode);

                            // Now go through the rows yet again to set up overlay windows, but now the summmary is not
                            // in the tbody. In this if there is no data the use the empty list which will prevent processing.

                            
                            rows = [.. tableNode.SelectNodes("./tbody//tr") ?? Enumerable.Empty<HtmlNode>()];
                            string imagePath = "../PlayerPhotos/";
                            foreach (HtmlNode row in rows)
                            {
                                HtmlNode nameCell = row.SelectSingleNode("./td[2]");
                                nameCell.SetAttributeValue("style", "cursor:pointer");
                                string playerName = nameCell.InnerText;

                                // Put this in utilities and share with GamesTeamsPlayersV3
                                Dictionary<string, string> map = PlayerPhotos.GetPlayerName2ImageNameMap();

                                string playerKey = string.Empty;
                                string[] playerNameSplit = playerName.Split(' ');
                                string firstName = playerNameSplit[0];
                                
                                if (playerNameSplit.Length == 2)
                                {
                                    playerKey = $"{playerNameSplit[1]}, {playerNameSplit[0]}";
                                }
                                else
                                {
                                    playerKey = $"Unknown, {string.Empty}";
                                }

                                if (!map.TryGetValue(playerKey, out string? imageName))
                                {
                                    imageName = "Available_Photo-Not";
                                }
                                // End of code to find photo name.

                                //string child = $"""
                                //               <div class="overlaystats"><img style="width:240px;" src="../PlayerPhotos/{imageName}.jpg"></div> 
                                //               """;
                                int avgTdIndex = Utilities.GetTableColumnIndex(tableNode, "AVG");
                                List<HtmlNode> computedTdElements = row.SelectNodes(".//td").Skip(avgTdIndex - 1).Take(4).ToList();
                                List<string> statValues = computedTdElements.Select(t => t.InnerText).ToList();
                                
                                IEnumerable<HtmlNode> tdElements = row.SelectNodes(".//td[contains(@class, 'zScore')]");
                                IEnumerable<string> zScores = tdElements.Select(n => n.InnerText); //.ToString<string>("<br/>");
                                string overlayHtml = StaticConstants.BuildOverlayStats(imagePath,
                                                                                       imageName,
                                                                                       firstName,
                                                                                       playerName,
                                                                                       statValues,
                                                                                       [.. zScores]);
                                HtmlNode popUp = HtmlNode.CreateNode(overlayHtml);
                                nameCell.ChildNodes.Append(popUp);
                                nameCell.Attributes.Add("onmouseover", "this.querySelector('.overlay').style.display='block'");
                                nameCell.Attributes.Add("onmouseout", "this.querySelector('.overlay').style.display='none'");
                            }

                                headerText = $"All Players Stats for {leaguePlayersStatistics.ToList()[listIndex].League.ShortLeagueName}";
                            break;
                        }
                        case 2:
                        {
                            //tableNode.ParentNode.SetAttributeValue("style", "display:none");
                            tableNode.ParentNode.AddClass("hidden");
                            tableNode.AddClass("statistics");
                            tableNode.SetAttributeValue("style", "display:table");

                            List<HtmlNode> firstColumn = [..tableNode.SelectNodes("./tbody//tr/td[1]")];
                            List<string> statNames = Query.ComputedStatDisplayNames;
                            for (int i = 0; i < firstColumn.Count; i++)
                            {
                                firstColumn[i].InnerHtml = statNames[i];
                            }

                            List<HtmlNode> tableColumnHeaders = Utilities.GetTableColumnHeaders(tableNode);
                            tableColumnHeaders[0].SetAttributeValue("title", "The stats for which the statistics are created.");
                            tableColumnHeaders[1].SetAttributeValue("title", "The smallest value for each of the stats");
                            tableColumnHeaders[2].SetAttributeValue("title", "The largest value for each of the stats");
                            tableColumnHeaders[3].SetAttributeValue("title", "The mean value for each of the stats");
                            tableColumnHeaders[4].SetAttributeValue("title", "The value for which half the stats are smaller (that is, the 50th percentile");
                            tableColumnHeaders[5].SetAttributeValue("title", "The variance of all values for each of the stats");
                            tableColumnHeaders[6].SetAttributeValue("title", "The standard deviation of all values for each of the stats");
                            tableColumnHeaders[7].SetAttributeValue("title", "The number of players whose stats are used to compute the weighted statistics");
                            headerText = $"{leaguePlayersStatistics.ToList()[listIndex].League.ShortLeagueName} Weighted Statistics";
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }

                    
                }
                else if (t.Depth() == 0)
                {
                    headerText = isSummaryTables ? "Sortable Summary Tables for Leagues and Players" : "Sortable Tables for All Players Stats for Each League";
                    Utilities.ExcludeTableColumn(tableNode, 1);
                    tableNode.SelectSingleNode("./thead/tr[2]").Remove();
                    if (!string.IsNullOrEmpty(headerCssStyle))
                    {
                        tableNode.SelectSingleNode("./thead/tr[1]").Attributes.Add("style", headerCssStyle);
                    }
                }

                return $"{headerText}"; //&mdash; {headerTableInfo}";
            };
        }
    }
}
