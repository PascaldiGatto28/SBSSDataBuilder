// Ignore Spelling: Linq

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
    public class GamesTeamPlayersV3 : IHtmlCreator
    {
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "All data for games, teams and players"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["Games, Teams & Players", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        public GamesTeamPlayersV3()
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
            Action<object> actionCallback = callback == null ? (v) => Console.WriteLine(v.ToString()) : callback;
            string season = seasonText.RemoveWhiteSpace();

            string changedHtml = string.Empty;

            Assembly assembly = typeof(GamesTeamPlayersV3).Assembly;
            string resName = assembly.FormatResourceName(ResourceName);
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();

            string path = $"{dataStoreFolder}{season}LeaguesData.json";
            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(path))
            {

                Query query = new Query(dsContainer);
                //DataStoreInformation dsInfo = new DataStoreInformation(dsContainer);
                //string dsInfoHeaderStyle = "background-color:#d62929;";
                string gtpHeaderStyle = "background-color:#d62929; width:642px;";

                IEnumerable<Game> playedGames = query.GetPlayedGames();
                var leagueNames = query.GetLeagueDescriptions().OrderBy(d => d.LeagueCategory).Select(l => new
                {
                    Day = l.LeagueDay,
                    Category = l.LeagueCategory,
                    FullLeagueName = l.ToString(),
                    ShortLeagueNume = $"{l.LeagueDay} {l.LeagueCategory}"
                });

                using (HtmlGenerator generator = new HtmlGenerator())
                {
                    string expandCollapseHtml = """
                                                <div>
                                                     <button class="sbss" onclick = "viewAll(true)">Expand All Tables</button>
                                                     <button class="sbss" onclick = "viewAll(false)">Collapse All Tables</button>
                                                </div> 
                                                """;
                    //string displayRankingColumn = """
                    //                               <script type=text/javascript>
                    //                                   function setColumn (checked)
                    //                                   {
                    //                                        alert(checked);
                    //                                   }
                    //                               </script>  
                    //                               <div style="text-align:right;">
                    //                                   <input type="checkbox" id="rankingColumn" name="ranking" value="true" onchange="setColumn(this.checked);">
                    //                                   <label for="ranking">Hide the ranking column</label>
                    //                               </div>
                    //                               """;
                    generator.WriteRawHtml(expandCollapseHtml);

                    //actionCallback("expandCollapseHtml raw Html written");

                    //generator.WriteRootTable(dsInfo, LinqPadCallbacks.ExtendedDsInfo(dsInfoHeaderStyle));
                    //actionCallback("dsInfo root table written");

                    foreach (var leagueName in leagueNames)
                    {
                        IEnumerable<Game> leagueGames = playedGames.Where(g => (g.GameInformation.LeagueDay == leagueName.Day) &&
                                                                               (g.GameInformation.LeagueCategory == leagueName.Category));

                        if (leagueGames.Any())
                        {

                            var games = leagueGames.Select(g => new
                            {
                                Games = new GameInformationDisplay(g.GameInformation),
                                Teams = g.Teams.Select(t => new TeamStatsDisplay(new TeamStats(t))),
                            });

                            string fullLeagueName = leagueName.FullLeagueName;
                            string shortLeagueName = $"{leagueName.Day} {leagueName.Category}";

                            IEnumerable<TeamSummaryStatsDisplay> tss = query.GetTeamsPlayersStats(leagueName.Category, leagueName.Day)
                                                                            .Select(t => new TeamSummaryStatsDisplay(t));
                            IEnumerable<PlayerStatsRank> playerStatsRanks = query.GetLeaguePlayerStatsRank(leagueName.Category, leagueName.Day);//.Dump();
                            IEnumerable<PlayerStatsRankDisplay> playerStatRankDisplay =
                                             playerStatsRanks.Select(psr => new PlayerStatsRankDisplay(psr.Player, psr.PlayerRank));

                            var gtp = new
                            {
                                GamesAndTeams = games,
                                TeamPlayers = tss,
                                //HideRank = Util.RawHtml(displayRankingColumn),
                                Players = playerStatRankDisplay
                            };

                            actionCallback(this);

                            generator.WriteRootTable(gtp, LinqPadCallbacks.ExtendedGamesTeamPlayers($"{fullLeagueName}", gtpHeaderStyle));
                        }

                    }

                    string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    changedHtml = generator.DumpHtml(pageTitle: title,
                                                     //cssStyles: StaticConstants.LocalStyles,
                                                     //javaScript: StaticConstants.LocalJavascript,
                                                     cssStyles: StaticConstants.LocalStyles + StaticConstants.HelpStyles,
                                                     javaScript: StaticConstants.LocalJavascript + StaticConstants.HelpJavascript,
                                                     collapseTo: 1,
                                                     headElements: headElements.ToList());
                }

            }

            return changedHtml;
        }
    }
}