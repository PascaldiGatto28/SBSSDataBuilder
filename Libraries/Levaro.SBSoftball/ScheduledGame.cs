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

        [JsonIgnore]
        public bool IsComplete => VisitorScore.HasValue && HomeScore.HasValue;

        public Game GameResults
        {
            get;
            set;
        }

        public override string ToString()
        {
            StringBuilder summary = new();
            if (IsComplete)
            {
                DateTime gameTime = GameResults.GameInformation.Date;
                summary.Append($"{gameTime,-11:MM/dd/yyyy}{gameTime,8:h:mm tt}  ");
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