﻿// Ignore Spelling: Linq

using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using Query = SBSSData.Softball.Stats.Query;


namespace SBSSData.Application.LinqPadQuerySupport
{
    public class GamesTeamPlayersV4
    {
        private static readonly string SBSSExpand = """
                                                        button.SBSSexpand {
                                                        cursor:pointer; 
                                                        font-weight:500; 
                                                        font-size:12px;
                                                        font-family:'Segoe UI Semibold', 'sans serif';
                                                        border:1px solid black; 
                                                        padding:2px 5px 5px 5px; 
                                                        margin-right:10px;
                                                        color:white;
                                                        background-color: #4C74b2;
                                                        }
                                                        
                                                        #overlay
                                                        {
                                                        position: fixed; 
                                                        display: block; 
                                                        width: 290px; 
                                                        height: 500px;
                                                        top: 200px;
                                                        left: 245px;
                                                        right: 0;
                                                        bottom: 0;
                                                        background-color: rgba(220,220,220, .9); 
                                                        z-index: 9; 
                                                        cursor: pointer; 
                                                        border-radius:25px;
                                                        padding:25px;
                                                        }
                                                    """;
        public GamesTeamPlayersV4()
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

            string changedHtml = string.Empty;

            Assembly assembly = typeof(GamesTeamPlayersV4).Assembly;
            string resName = assembly.FormatResourceName("GamesTeamPlayersIntro.html");
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();

            string path = $"{dataStoreFolder}{season}LeaguesData.json";
            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(path))
            {
                Query query = new Query(dsContainer);
                using (HtmlGenerator generator = new HtmlGenerator())
                {
                    string expandCollapseHtml = """
                                                <div>
                                                     <button class="SBSSexpand" onclick = "viewAll(true)">Expand All Tables</button>
                                                     <button class="SBSSexpand" onclick = "viewAll(false)">Collapse All Tables</button>
                                                </div> 
                                                """;
                    generator.WriteRawHtml(expandCollapseHtml);
                    actionCallback(expandCollapseHtml);

                    IEnumerable<PlayerStatsDisplay> playersStats = query.GetLeaguePlayersSummary("Community", "Friday")
                                                                        .Select(ps => new PlayerStatsDisplay(ps));

                    actionCallback(playersStats);
                    generator.WriteRootTable(playersStats, LinqPadCallbacks.ExtendedGamesTeamPlayers("Friday Community Winter 2024"));

                    string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    changedHtml = generator.DumpHtml(pageTitle: title, cssStyles: SBSSExpand, collapseTo: 2);
                }

            }

            return changedHtml;
        }
    }
}