using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using Query = SBSSData.Softball.Stats.Query;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class SortablePlayerStats : IHtmlCreator
    {
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "Player league stats for all players and leagues using sortable tables"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["Player's League Sortable Summaries", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        public SortablePlayerStats()
        {
            Values = [];
            ResourceName = $"{this.GetType().Name}.html";
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
                Query query = new Query(dsContainer);

                using (HtmlGenerator generator = new())
                {
                    string expandCollapseHtml = """
                                                    <div>
                                                         <button class="sbss" onclick = "viewAll(true)">Expand All Tables</button>
                                                         <button class="sbss" onclick = "viewAll(false)">Collapse All Tables</button>
                                                         <button class="sbss" onclick = "toggleStatisticsDisplay()">Display Statistics</button>
                                                    </div> 
                                                    """;
                    string headerCss = """
                                        background-color:#4c74b2;
                                        font-weight:500;
                                        color: white;
                                        font-size:1.20em;
                                        """;

                    generator.WriteRawHtml(expandCollapseHtml);
                    IEnumerable<LeagueName> leagueNames = query.GetLeagueNames();
                    List<PropertyInfo> properties = typeof(PlayerStatsDisplay).GetProperties()
                                                                              .Where(p => Query.ComputedStatDisplayNames.Contains(p.Name))
                                                                              .ToList();

                    List<LeagueStatsStatistics> leagueStatsStatistics = [];
                    foreach (LeagueName league in leagueNames)
                    {
                        LeagueStatsStatistics lss = GetLeagueStatsStatistics(query, league.Category, seasonText, properties, league.Day);
                        leagueStatsStatistics.Add(lss);
                    }

                    generator.WriteRootTable(leagueStatsStatistics, ExtendedSortablePlayerStats(headerCss, leagueStatsStatistics));

                    leagueStatsStatistics = [];
                    IEnumerable<string> categoryNames = leagueNames.GroupBy(l => l.Category).Where(g => g.Count() > 1).Select(g => g.Key);
                    foreach (string category in categoryNames)
                    {
                        LeagueStatsStatistics lss = GetLeagueStatsStatistics(query, category, seasonText, properties);
                        leagueStatsStatistics.Add(lss);
                    }


                    generator.WriteRootTable(leagueStatsStatistics, ExtendedSortablePlayerStats(headerCss, leagueStatsStatistics));

                    string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    html = generator.DumpHtml(pageTitle: title,
                                              cssStyles: StaticConstants.SortableTableStyles + StaticConstants.LocalStyles, 
                                              javaScript:StaticConstants.LocalJavascript,
                                              collapseTo: 1,
                                              headElements: headElements.ToList());

                    if (callback != null)
                    {
                        callback("Sortable Player Stats constructed");
                    }
                }
            }

            return html;
        }

        public LeagueStatsStatistics GetLeagueStatsStatistics(Query query, string category, string seasonText, List<PropertyInfo> properties, string day = "")
        {
            string season = Utilities.SwapSeasonText(seasonText);
            LeagueName league = new LeagueName(day, category, $"{day} {category} {season}", $"{day} {category}");
            IEnumerable<PlayerStatsDisplay> playersStats = query.GetLeaguePlayersSummary(league.Category, league.Day)
                                                                        .Select(p => new PlayerStatsDisplay(p));

            List<StatisticsDisplay> sdList = [];
            IEnumerable<double> weights = playersStats.Select(ps => (double)ps.PA);
            foreach (PropertyInfo property in properties)
            {
                IEnumerable<double> values = playersStats.Select(ps => (double)property.GetValue(ps));
                string stateName = Query.ComputedStatNames[Query.ComputedStatDisplayNames.IndexOf(property.Name)];
                StatisticsDisplay sd = new StatisticsDisplay(values.GetStatistics(stateName.NameToTitle(), weights));
                sdList.Add(sd);
            }

            LeagueStatsStatistics lss = new LeagueStatsStatistics(league, playersStats, sdList);
            return lss;
        }

        public static Func<TableNode, string> ExtendedSortablePlayerStats(string? headerCssStyle = null, object? value = null)
        {
            return (t) =>
            {
                IEnumerable<LeagueStatsStatistics> leagueStatsStatistics = (IEnumerable<LeagueStatsStatistics>)value;
                bool isSummaryTables = string.IsNullOrEmpty(leagueStatsStatistics.First().League.Day);
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
                            var headerNodes = tableNode.SelectNodes(".//th").ToList();
                            List<int> stringElementList = [];
                            for (int i = 0; i < headerNodes.Count(); i++)
                            {
                                HtmlNode headerNode = headerNodes[i];
                                if (headerNode.GetAttributeValue("title", null) == "System.String")
                                {
                                    stringElementList.Add(i);
                                }
                            }

                            string stringElements = $"""'[{stringElementList.ToString<int>(", ")}]'""";
                            string tableSelector = $"'#{t.Id()}'";

                            Utilities.UpdatePlayerColumnNames(tableNode);
                            Utilities.UpdatePlayerHeaderTitles(tableNode, true, false);
                            string eventScript = StaticConstants.SortableTable.Replace("[[tableSelector]]", tableSelector).Replace("[[stringElements]]", stringElements);
                            HtmlNode script = HtmlNode.CreateNode(eventScript);
                            tableNode.ParentNode.InsertAfter(script, tableNode);

                            headerText = $"All Players Stats for {leagueStatsStatistics.ToList()[listIndex].League.ShortLeagueName}";
                            break;
                        }
                        case 2:
                        {
                            tableNode.AddClass("statistics");
                            tableNode.SetAttributeValue("style", "display:table");
                            headerText = $"Summary {leagueStatsStatistics.ToList()[listIndex].League.ShortLeagueName} Leagues Descriptive Statistics";
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

                return $"{headerText} &mdash; {headerTableInfo}";
            };
        }
    }
}
