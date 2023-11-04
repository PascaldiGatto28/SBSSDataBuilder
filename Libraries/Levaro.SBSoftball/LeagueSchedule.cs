using HtmlAgilityPack;

using Levaro.SBSoftball.Common;

namespace Levaro.SBSoftball
{
    public class LeagueSchedule
    {
        public LeagueSchedule()
        {
            LeagueDescription = new();
            ScheduledGames = Enumerable.Empty<ScheduledGame>();
        }

        public LeagueDescription LeagueDescription
        {
            get;
            set;
        }

        public IEnumerable<ScheduledGame> ScheduledGames
        {
            get;
            set;
        }

        public static LeagueSchedule ConstructLeagueSchedule(string leagueScheduleLocation)
        {
            LeagueSchedule leagueSchedule = new();
            LeagueDescription leagueDescription = new();

            Uri uri = new(leagueScheduleLocation);
            HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(uri);
            leagueDescription.ScheduleDataSource = uri;

            HtmlNode article = htmlDocument.DocumentNode.SelectSingleNode("//article");
            string articleClass = article.GetAttributeValue("class", string.Empty);
            string[] leagueInfo = articleClass.Substring("sp_league-", "-league", false, false)
                                              .Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            leagueDescription.LeagueDay = leagueInfo[0].Trim().Capitalize();
            leagueDescription.LeagueCategory = leagueInfo[1].Trim().Capitalize();


            string[] leagueSeason = articleClass.Substring("sp_season", false)
                                                .Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            leagueDescription.Season = leagueSeason[0].Trim().Capitalize();
            leagueDescription.Year = leagueSeason[1].Trim();
            leagueSchedule.LeagueDescription = leagueDescription;

            // Now the individual games that are part of the schedule for this league

            //HtmlDocument workingHtmlDoc = PageContentUtilities.GetHtmlDocument(article);
            IEnumerable<HtmlNode> rows = article.SelectNodes("//table/tbody/tr").Cast<HtmlNode>();
            //rows.Select(n => n.Attributes["class"].Value); //.Dump();
            List<ScheduledGame> scheduledGames = new();
            foreach (HtmlNode row in rows)
            {
                // TODO: Need to decode html entities in the team names - DONE
                ScheduledGame scheduledGame = new()
                {
                    Date = DateTime.Parse(row.SelectSingleNode("td/a/date").InnerText),

                    VisitingTeamName = row.SelectSingleNode("td[@class='data-home']").InnerText.CleanNameText(),
                    HomeTeamName = row.SelectSingleNode("td[@class='data-away']").InnerText.CleanNameText()
                };

                HtmlNode resultsHtmlNode = row.SelectSingleNode("td[@class='data-results']/a");
                scheduledGame.ResultsUrl = new Uri(resultsHtmlNode.GetAttributeValue("href", string.Empty));
                string scoreText = resultsHtmlNode.InnerText;
                string[] score = scoreText.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                if (score.Length == 0)
                {
                    scheduledGame.VisitorScore = null;
                    scheduledGame.HomeScore = null;
                }
                else if (score.Length > 1)
                {
                    scheduledGame.VisitorScore = int.Parse(score[0].Trim());
                    scheduledGame.HomeScore = int.Parse(score[1].Trim());
                }

                scheduledGames.Add(scheduledGame);

                if (scheduledGame.IsComplete)
                {
                    scheduledGame.GameResults = new Game(scheduledGame.ResultsUrl);
                }
            }

            leagueSchedule.ScheduledGames = scheduledGames;


            return leagueSchedule;
        }
    }
}
