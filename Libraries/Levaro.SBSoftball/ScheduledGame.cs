using System.Text;

using Newtonsoft.Json;

namespace Levaro.SBSoftball
{
    public class ScheduledGame
    {
        // TODO: This should be set by some configuration rather than hard-coded in the class.
        private static readonly int checkHours = 16;

        public ScheduledGame()
        {
            Date = DateTime.MaxValue;
            VisitingTeamName = string.Empty;
            HomeTeamName = string.Empty;
            GameResults = new();
        }

        public DateTime Date
        {
            get;
            init;
        }
        public string VisitingTeamName
        {
            get;
            init;
        }
        public string HomeTeamName
        {
            get;
            init;
        }
        public Uri? ResultsUrl
        {
            get;
            init;
        }
        public int? VisitorScore
        {
            get;
            set;
        }
        public int? HomeScore
        {
            get;
            set;
        }
        public Game GameResults
        {
            get;
            set;
        }

        private int CheckHours => checkHours;


        [JsonIgnore]
        public bool IsRecorded => Recorded();

        [JsonIgnore]
        public bool IsComplete => VisitorScore.HasValue && HomeScore.HasValue;

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

        public override string ToString()
        {
            StringBuilder summary = new();
            if (IsComplete)
            {
                DateTime gameTime = GameResults.GameInformation.Date;
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