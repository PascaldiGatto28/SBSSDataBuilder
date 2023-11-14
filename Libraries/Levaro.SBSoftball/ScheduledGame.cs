using System.Text;

using Newtonsoft.Json;

namespace Levaro.SBSoftball
{
    public class ScheduledGame
    {
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
            set;
        }
        public string VisitingTeamName
        {
            get;
            set;
        }
        public string HomeTeamName
        {
            get;
            set;
        }
        public Uri? ResultsUrl
        {
            get;
            set;
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

        [JsonIgnore]
        public int CheckHours => 16;

        [JsonIgnore]
        public bool IsComplete => VisitorScore.HasValue && HomeScore.HasValue;

        [JsonIgnore]
        public bool WasCancelled => (VisitorScore == 0) && (HomeScore == 0);

        /// <summary>
        /// Indicates whether the game has been played and recorded by the SBSS supervisor.
        /// </summary>
        /// <remarks>
        /// The assumption is that the information is available web site no later <paramref name="checkHours"/>
        /// less the hour of game start hours. At that point, if there is no team data, it is assumed that is cancelled 
        /// and not played.
        /// </remarks>
        /// <param name="checkHours">the number of hours from the beginning of the day. The parameter is option, and if
        /// <c>null</c>, the value of <see cref="CheckHours"/> is used.</param>
        /// <returns><c>true</c> if it has been determine number of hours after the start of the game.</returns>
        /// <seealso cref="LeagueSchedule.ConstructLeagueSchedule(string)"/>
        public bool IsRecorded(int? checkHours = null)
        {
            int hours = checkHours ?? CheckHours;
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