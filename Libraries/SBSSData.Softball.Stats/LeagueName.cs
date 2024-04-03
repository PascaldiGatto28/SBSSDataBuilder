namespace SBSSData.Softball.Stats
{
    public record LeagueName(string Day, string Category, string FullLeagueName, string ShortLeagueName)
    {
        public LeagueName(LeagueDescription leagueDescription) : this(leagueDescription.LeagueDay,
                                                                      leagueDescription.LeagueCategory,
                                                                      leagueDescription.ToString(),
                                                                      $"{leagueDescription.LeagueDay} {leagueDescription.LeagueCategory}"
                                                                     )
        {
        }

        //public override bool Equals(object? obj)
        //{
        //    bool isEqual = false;
        //    if ((obj != null) && (GetType() == obj.GetType()))
        //    {
        //        LeagueName leagueName = (LeagueName)obj;
        //        isEqual = string.Equals(Category, leagueName.Category, StringComparison.OrdinalIgnoreCase) &&
        //                  string.Equals(Day, leagueName.Day, StringComparison.OrdinalIgnoreCase) &&
        //                  string.Equals(FullLeagueName, leagueName.FullLeagueName, StringComparison.OrdinalIgnoreCase)

        //    }

        //    return isEqual;
        //}
    }
}
