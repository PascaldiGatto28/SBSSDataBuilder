using System.Text.Json.Serialization;

namespace Levaro.SBSoftball
{

    public class GameInformation
    {
        public GameInformation()
        {
            Title = string.Empty;
            GameId = string.Empty;
            LeagueCategory = string.Empty;
            LeagueDay = string.Empty;
            Season = string.Empty;
        }

        public string Title
        {
            get;
            set;
        }

        public string GameId
        {
            get;
            set;
        }

        public Uri? DataSource
        {
            get;
            set;
        }

        public DateTime Date
        {
            get;
            set;
        }

        public string LeagueCategory
        {
            get;
            set;
        }

        public string LeagueDay
        {
            get;
            set;
        }

        public string Season
        {
            get;
            set;
        }

        [JsonIgnore]
        public string Year => $"{Date.Date:yyyy}";

        public override string ToString()
        {
            string description = "No description available";
            if (!string.IsNullOrEmpty(Title))
            {
                description = $"{Title} [{Season} {LeagueDay} {LeagueCategory}] on {Date:dddd MMMM, d yyyy}";
            }

            return description;
        }
    }
}
