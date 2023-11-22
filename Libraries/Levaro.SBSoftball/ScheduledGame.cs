using System.Text;

using HtmlAgilityPack;

using Levaro.SBSoftball.Common;

using Newtonsoft.Json;

namespace Levaro.SBSoftball
{
    /// <summary>
    /// Encapsulates the data about the scheduled game, for example, the scheduled game date, team names, page Url that
    /// contains information for the game, and for completed games, team score, and team and player stats.
    /// </summary>
    public sealed class ScheduledGame
    {
        // TODO: This should be set by some configuration rather than hard-coded in the class.
        private static readonly int checkHours = 16;

        private ScheduledGame()
        {
            Date = DateTime.MaxValue;
            VisitingTeamName = string.Empty;
            HomeTeamName = string.Empty;
            //GameResults = new();
        }

        /// <summary>
        /// Constructs an instance using data scraped from league schedule page.
        /// </summary>
        /// <remarks>
        /// This method is called by the <see cref="LeagueSchedule.ConstructLeagueSchedule(string)"/> method, which recovers the
        /// source page for the league schedule specified by the parameter. For example
        /// <para>
        /// https://saddlebrookesoftball.com/calendar/monday-recreation-schedule/
        /// </para>
        /// The main table on the page consists of rows representing each of the scheduled games for the league. This is the
        /// table from which the information for each scheduled game is recovered.
        /// </remarks>
        /// <param name="row">
        /// The <see cref="HtmlAgilityPack"/><see cref="HtmlNode"/> object that has the HTML for the scheduled game.
        /// </param>
        /// <returns>
        /// A <c>ScheduledGame</c> instance
        /// </returns>
        public static ScheduledGame ConstructionScheduleGame(HtmlNode row)
        {
            ScheduledGame scheduledGame;
            try
            {
                HtmlNode resultsHtmlNode = row.SelectSingleNode("td[@class='data-results']/a");
                Uri? resultsUrl = new(resultsHtmlNode.GetAttributeValue("href", string.Empty));

                scheduledGame = new()
                {
                    Date = DateTime.Parse(row.SelectSingleNode("td/a/date").InnerText),
                    VisitingTeamName = row.SelectSingleNode("td[@class='data-home']").InnerText.CleanNameText(),
                    HomeTeamName = row.SelectSingleNode("td[@class='data-away']").InnerText.CleanNameText(),
                    ResultsUrl = resultsUrl
                };

                string scoreText = resultsHtmlNode.InnerText;
                string[] score = scoreText.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);

                if (score.Length > 1)
                {
                    scheduledGame.VisitorScore = int.Parse(score[0].Trim());
                    scheduledGame.HomeScore = int.Parse(score[1].Trim());
                }

                // Use the data from the game results page to construct the game information. The scheduled game ResultsUrl is
                // used to access the game information, team and player stats.
                scheduledGame.GameResults = Game.ConstructGame(scheduledGame, update: false); ;

                // Setting the scores even though there is no team/player data indicates that the game was cancelled.
                if (scheduledGame.IsRecorded && !scheduledGame.IsComplete)
                {
                    scheduledGame.HomeScore = 0;
                    scheduledGame.VisitorScore = 0;
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Unable to parse the row of the page containing the scheduled game.", exception);
            }

            return scheduledGame;
        }

        /// <summary>
        /// Gets and initializes the schedule time and date for the game.
        /// </summary>
        public DateTime Date
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the visiting team name.
        /// </summary>
        /// <remarks>
        /// Any encoded HTML in the name is decoded during initialization.
        /// </remarks>
        public string VisitingTeamName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the home team name.
        /// </summary>
        /// <remarks>
        /// Any encoded HTML in the name is decoded during initialization.
        /// </remarks>
        public string HomeTeamName
        {
            get;
            init;
        }


        /// <summary>
        /// Gets and initializes the <see cref="Uri"/> of the page having all the game information and stats.
        /// </summary>
        /// <remarks>
        /// This is the <c>Uri</c> that the static <see cref="Game.ConstructGame(ScheduledGame, bool)"/> method to recover
        /// the game data. For example, <c>https://saddlebrookesoftball.com/event/26588/</c>.
        /// </remarks>
        public Uri? ResultsUrl
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and sets the visiting team's score.
        /// </summary>
        /// <remarks>
        /// Because the value can only be set after the game is played or cancelled, it is initialized to <c>null</c>.
        /// </remarks>
        public int? VisitorScore
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the home team's score.
        /// </summary>
        /// <remarks>
        /// Because the value can only be set after the game is played or cancelled, it is initialized to <c>null</c>.
        /// </remarks>
        public int? HomeScore
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the <see cref="Game"/> instance that encapsulates the game information, results and stats.
        /// </summary>
        /// <remarks>
        /// From the <see cref="ConstructionScheduleGame(HtmlNode)"/> static method, the 
        /// <see cref="Game.ConstructGame(ScheduledGame, bool)"/> is call which always initializes the 
        /// <see cref="Game.GameInformation"/> property and which never changes. For a scheduled that hasn't completed 
        /// the <see cref="Game.Teams"/> property is left "empty" until the game is completed at which it is updated.
        /// </remarks>
        public Game? GameResults
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the value of the readonly field <c>checkHours</c> which is used to help determine if a game should be
        /// marked as cancelled.
        /// </summary>
        private static int CheckHours => checkHours;


        /// <summary>
        /// Gets the value that indicates if the results of the game have included in the SBSS Web site.
        /// </summary>
        /// <remarks>
        /// If <c>true</c>, the results should have updated in the Web site; otherwise the game has not been played yet or there
        /// has been time to update the site as yet.
        /// </remarks>
        [JsonIgnore]
        public bool IsRecorded => Recorded();

        /// <summary>
        /// Get a <c>bool</c> value indicating whether the game has played (or cancelled).
        /// </summary>
        /// <remarks>
        /// <c>true</c> if the game has been played (or cancelled); false other wise.
        /// </remarks>
        /// <seealso cref="Recorded()"/>
        [JsonIgnore]
        public bool IsComplete => VisitorScore.HasValue && HomeScore.HasValue;

        /// <summary>
        /// Returns <c>true</c> if the game has been cancelled.
        /// </summary>
        /// <remarks>
        /// A game is cancelled if the game has been recorded (<see cref="IsRecorded"/> is <c>true</c>, yet there is no team data
        /// indicating the game has not bee played, that is, it has been cancelled. This is indicated by setting the visitor and 
        /// home scores to 0.
        /// </remarks>
        [JsonIgnore]
        public bool WasCancelled => (VisitorScore == 0) && (HomeScore == 0);

        /// <summary>
        /// Indicates whether the game results have been recorded by the SBSS supervisor.
        /// </summary>
        /// <remarks>
        /// The assumption is that the information is available from the SBSS Web site no later <see cref="CheckHours"/>
        /// (the minimal value of which is 16) less the hour of game start time. At that point, if there is no team data, 
        /// it is assumed that the game has been cancelled and will not be played.
        /// </remarks>
        /// <returns><c>true</c> if it has been more than number of hours after the start time of the game.</returns>
        /// <seealso cref="LeagueSchedule.ConstructLeagueSchedule(string)"/>
        private bool Recorded()
        {
            int hours = Math.Max(CheckHours, 16);
            DateTime recordedTime = Date.AddHours(hours - Date.Hour);
            return (recordedTime < DateTime.Now);
        }

        /// <summary>
        /// Returns a string representation of meta data for this instance of <see cref="ScheduledGame"/>.
        /// </summary>
        /// <returns>For a completed game, the date and followed by team names and final score. For example
        /// <code>
        /// 10/09/2023 8:00 AM  The Charles Company (Visitors, Runs 9, Loss) vs Tucson Orthopaedic Institute (Home, Runs 13, Win)
        /// </code>
        /// It the game is not complete, the string "Not yet played" is returned.
        /// </returns>
        public override string ToString()
        {
            StringBuilder summary = new();
            if (IsComplete && (GameResults != null))
            {
                DateTime gameTime = GameResults.GameInformation?.Date ?? DateTime.MinValue;
                summary.Append($"{gameTime,-11:MM/dd/yyyy}{gameTime:h:mm tt}  ");
                List<Team> teams = GameResults.Teams.ToList();
                summary.Append(teams[0].ToString()).Append(" vs ").Append(teams[1].ToString());
            }
            else
            {
                summary.AppendLine("Not yet played");
            }

            return summary.ToString();
        }
    }
}