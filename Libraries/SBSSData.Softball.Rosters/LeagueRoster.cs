using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Softball.Rosters
{
    public class LeagueRoster
    {
        public LeagueRoster()
        {
            League = LeagueName.Empty;
            TeamRosters = [];
        }

        public LeagueRoster(LeagueName league, List<TeamRoster> teamRosters)
        {
            League = league;
            TeamRosters = teamRosters;
        }
        public LeagueName League
        {
            get; set;
        }
        public List<TeamRoster> TeamRosters
        {
            get; set;
        }

        public static LeagueRoster Empty => new LeagueRoster();

        public static LeagueRoster ConstructLeagueRoster(LeagueName leagueName, Action<string>? message = null)
        {
            Action<string> callback = message == null ? (m) => Console.WriteLine(m) : message;

            LeagueRoster leagueRoster = Empty;
            string rosterUrl = $"https://saddlebrookesoftball.com/{leagueName.Day}-{leagueName.Category}-rosters/";

            List<TeamRoster> teamRosters = [];

            callback($"Processing {leagueName.FullLeagueName}");
            leagueRoster.League = leagueName;

            HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(new Uri(rosterUrl));
            HtmlNode root = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='entry-content']");

            try
            {
                List<HtmlNode> spanNodes = root.SelectNodes("//span").ToList();//.Count.Dump("Number of span nodes");
                int numSpanNodes = spanNodes.Count;
                var spNodes = root.SelectNodes("//div[@class='sportspress']");
                for (int i = 0; i < numSpanNodes; i++)
                {
                    HtmlNode spNode = spNodes.ElementAt(i);
                    string teamName = spNode.SelectSingleNode("h4").InnerText.CleanNameText();
                    string manager = spanNodes[i].InnerHtml.Substring("Manager: ".Length);

                    // There's one roster (monday community recreation, that is the first one in SSSA Rosters&Stats)
                    // that has a missing </span>. Normally, endIndex is -1 in which case the selecting just the
                    // appropriate text is not needed. If this ever gets fixed, the code will not execute and
                    // no harm no foul.
                    int endIndex = manager.IndexOf("<br");
                    if (endIndex > 0)
                    {
                        manager = manager.Substring(0, endIndex);
                        callback("Fixed Manager name");
                    }

                    HtmlNode tableBody = spNode.SelectSingleNode("div/div/table/tbody");
                    IEnumerable<string> players = tableBody.SelectNodes("tr/td[@class='data-name']").Select(n => n.InnerText.CleanNameText());
                    TeamRoster roster = new(teamName, manager, players.ToList());
                    teamRosters.Add(roster);
                }

                leagueRoster.TeamRosters = teamRosters;
            }
            catch (NullReferenceException exception)
            {
                callback(exception.Message);
                throw;
            }

            return leagueRoster;
        }
    }
}
