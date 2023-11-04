namespace Levaro.SBSoftball
{
    public class LeagueDescription
    {
        public LeagueDescription()
        {
            LeagueCategory = string.Empty;
            LeagueDay = string.Empty;
            Season = string.Empty;
            Year = string.Empty;
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

        public string Year
        {
            get;
            set;
        }

        public Uri? ScheduleDataSource
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"{LeagueDay} {LeagueCategory} {Season} {Year}";
        }
    }
}
