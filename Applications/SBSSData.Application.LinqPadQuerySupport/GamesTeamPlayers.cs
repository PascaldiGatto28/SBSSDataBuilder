// Ignore Spelling: Linq

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using Query = SBSSData.Softball.Stats.Query;


namespace SBSSData.Application.LinqPadQuerySupport
{
    public class GamesTeamPlayers
    {
        public GamesTeamPlayers()
        {
            Values = [];
        }

        public List<object> Values
        {
            get;
            set;
        }

        public string BuildHtmlPage(string seasonText, string dataStoreFolder)
        {
            string text = seasonText;
            string season = seasonText.RemoveWhiteSpace();
            string changedHtml = string.Empty;


            string dataStorePath = $@"{dataStoreFolder}{season}LeaguesData.json";
            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath))
            {
                Query query = new Query(dsContainer);

                DataStoreInformation dsInfo = new DataStoreInformation(dsContainer ?? DataStoreContainer.Empty);


                IEnumerable<Game> playedGames = query.GetPlayedGames();
                IEnumerable<string> leagueNames = query.GetLeagueDescriptions().OrderBy(d => d.LeagueCategory).Select(d => $"{d.LeagueDay} {d.LeagueCategory}");

                using (HtmlGenerator generator = new HtmlGenerator())
                {
                    generator.WriteRootTable(dsInfo, LinqPadCallbacks.DsInfoCallback);
                    Values.Add(dsInfo);

                    foreach (string leagueName in leagueNames)
                    {
                        IEnumerable<Game> leagueGames = playedGames.Where(g => $"{g.GameInformation.LeagueDay} {g.GameInformation.LeagueCategory}" == leagueName);
                        var games = leagueGames.Select(g => new
                        {
                            Games = new GameInformationDisplay(g.GameInformation),
                            Teams = g.Teams.Select(t => new TeamStatsDisplay(new TeamStats(t)))
                        });

                        generator.WriteRootTable(games, LinqPadCallbacks.GamesCallback);
                        Values.Add(games);
                    }

                    HtmlNode title = HtmlNode.CreateNode("""<span style="color:firebrick; font-size:1.25em; font-weight:500;">Games, Teams and Players</span>""");
                    changedHtml = generator.DumpHtml(pageTitle: title, collapseTo: 2);
                }
            }

            return changedHtml;
        }
    }
}
