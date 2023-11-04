namespace Levaro.SBSoftball
{
    public class LeaguesData
    {
        public LeaguesData()
        {
            BuildDate = DateTime.Now;
            LeagueSchedules = Enumerable.Empty<LeagueSchedule>();
        }

        public DateTime BuildDate
        {
            get;
            set;
        }

        public IEnumerable<LeagueSchedule> LeagueSchedules
        {
            get;
            set;
        }
    }
}
