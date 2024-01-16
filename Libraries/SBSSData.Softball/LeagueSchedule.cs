using HtmlAgilityPack;

namespace SBSSData.Softball
{
    /// <summary>
    /// Encapsulates all information about the scheduled games for a specific league whether the games have been played 
    /// (completed), cancelled or to be played in the future.
    /// </summary>
    /// <remarks>
    /// Instances should only be constructed using the static method <see cref="ConstructLeagueSchedule(string)"/> where
    /// the league name (key) is obtained from the <see cref="LeagueLocations"/> instance. 
    /// <see cref="LeaguesData.LeagueSchedules"/> property is a sequence of <c>LeagueSchedule</c> objects
    /// and are the essential information of the data store.
    /// <para>
    /// Because the default constructor is private and all property setters are "private init", instances of this class are
    /// essentially immutable, although because the <see cref="ScheduledGames"/> property is a sequence of 
    /// <see cref="ScheduledGame"/> objects, the individual scheduled game information is altered when the game is played or
    /// cancelled.
    /// </para>
    /// </remarks>
    /// <seealso cref="LeaguesData"/>
    /// <seealso cref="ScheduledGame"/>
    public sealed class LeagueSchedule
    {
        /// <summary>
        /// The private default constructor that initializes all properties to default values so that properties need not be
        /// set to <c>nullable</c>.
        /// </summary>
        private LeagueSchedule()
        {
            //LeagueDescription = new();
            ScheduledGames = Enumerable.Empty<ScheduledGame>();
            IsEmpty = true;
            LeagueDescription = LeagueDescription.Empty();
        }


        /// <summary>
        /// Constructs an instance with all properties set to initial values.
        /// </summary>
        /// <returns>An empty instance, that is, properties are set to default initial values.</returns>
        public static LeagueSchedule Empty()
        {
            return new LeagueSchedule();
        }

        /// <summary>
        /// Gets or initializes the <c>IsEmpty</c> property. 
        /// </summary>
        /// <remarks>
        /// The default constructor initializes the value to <c>true</c> and is only set
        /// to <c>false</c> when <see cref="ConstructLeagueSchedule(string)"/> successfully returns an instance.
        /// </remarks>
        public bool IsEmpty
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes the <c>LeagueDescription</c> property.
        /// </summary>
        /// <remarks>
        /// The default constructor initializes the value to the default instance, but then its <c>LeagueDescription</c>
        /// properties are initialized by the <see cref="ConstructLeagueSchedule(string)"/> method.
        /// </remarks>
        public LeagueDescription LeagueDescription
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the <c>ScheduledGames</c> property.
        /// </summary>
        /// <remarks>
        /// This property is to <c>LeagueSchedule</c> what the property <see cref="LeaguesData.LeagueSchedules"/> is to the
        /// <see cref="LeaguesData"/> class. A league schedule is just a sequence of scheduled games and all the detailed
        /// data resides with the <see cref="ScheduledGame"/> instances.
        /// </remarks>
        public IEnumerable<ScheduledGame> ScheduledGames
        {
            get;
            init;
        }

        /// <summary>
        /// Given a league schedule location, a <c>LeagueSchedule</c> instance is created.
        /// </summary>
        /// <param name="leagueScheduleLocation">
        /// The location of a league is the string representation of a <see cref="Uri"/> to a page having links to all the
        /// scheduled games. For example <c>https://saddlebrookesoftball.com/calendar/monday-recreation-schedule/</c> is the link
        /// to the schedule for the Monday Recreation league.
        /// </param>
        /// <remarks>
        /// Short of using reflection, this is the only way to create an instance of a <c>LeagueSchedule</c> class. From the
        /// page recovered using the <paramref name="leagueScheduleLocation"/> property, the contents are scraped to initialize
        /// the <see cref="LeagueSchedule.LeagueDescription"/> property, and follow on the schedule game links on the page to
        /// build the <see cref="ScheduledGames"/> property.
        /// </remarks>
        /// <returns>
        /// A <c>LeagueSchedule</c> instance. If <paramref name="leagueScheduleLocation"/> is <c>null</c> or empty, the empty
        /// <c>LeagueSchedule</c> instance (that is, <see cref="IsEmpty"/> is <c>true</c>) is returned. <c>null</c> is never
        /// returned.
        /// </returns>
        public static LeagueSchedule ConstructLeagueSchedule(string leagueScheduleLocation)
        {
            LeagueSchedule leagueSchedule = new();
            if (!string.IsNullOrEmpty(leagueScheduleLocation))
            {
                Uri uri = new(leagueScheduleLocation);
                HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(uri);
                LeagueDescription leagueDescription = LeagueDescription.ConstructionLeagueDescription(uri, htmlDocument);

                HtmlNode article = htmlDocument.DocumentNode.SelectSingleNode("//article");

                // Now the individual games that are part of the schedule for this league

                IEnumerable<HtmlNode> rows = article.SelectNodes("//table/tbody/tr").Cast<HtmlNode>();
                //rows.Select(n => n.Attributes["class"].Value); //.Dump();
                List<ScheduledGame> scheduledGames = [];
                foreach (HtmlNode row in rows)
                {
                    //HtmlNode resultsHtmlNode = row.SelectSingleNode("td[@class='data-results']/a");
                    //Uri? resultsUrl = new Uri(resultsHtmlNode.GetAttributeValue("href", string.Empty));

                    //ScheduledGame scheduledGame = new()
                    //{
                    //    Date = DateTime.Parse(row.SelectSingleNode("td/a/date").InnerText),

                    //    VisitingTeamName = row.SelectSingleNode("td[@class='data-home']").InnerText.CleanNameText(),
                    //    HomeTeamName = row.SelectSingleNode("td[@class='data-away']").InnerText.CleanNameText(),
                    //    ResultsUrl = resultsUrl
                    //};

                    //string scoreText = resultsHtmlNode.InnerText;
                    //string[] score = scoreText.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                    //if (score.Length > 1)
                    //{
                    //    scheduledGame.VisitorScore = int.Parse(score[0].Trim());
                    //    scheduledGame.HomeScore = int.Parse(score[1].Trim());
                    //}

                    //// Use the data from the game results page to construct the game information
                    //scheduledGame.GameResults = Game.ConstructGame(scheduledGame, update: false); ;

                    //// Setting the scores even though there is no team/player data indicates that the game was cancelled.
                    //if (scheduledGame.IsRecorded && !scheduledGame.IsComplete)
                    //{
                    //    scheduledGame.HomeScore = 0;
                    //    scheduledGame.VisitorScore = 0;
                    //}

                    // N.B. HTML entities in the team names are decoded using the CleanNameText extension method.
                    scheduledGames.Add(ScheduledGame.ConstructionScheduleGame(row));
                }

                leagueSchedule = new LeagueSchedule()
                {
                    IsEmpty = false,
                    LeagueDescription = leagueDescription,
                    ScheduledGames = scheduledGames
                };
            }

            return leagueSchedule;
        }
    }
}
