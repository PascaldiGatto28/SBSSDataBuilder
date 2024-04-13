namespace SBSSData.Application.Support
{

    public record PlayerSheetDisplay(string Description,
                                     int Teams,
                                     int Games,
                                     string Name,
                                     int PA,
                                     int AB,
                                     int R,
                                     int Singles,
                                     int Doubles,
                                     int Triples,
                                     int HR,
                                     int BB,
                                     int SF,
                                     int Hits,
                                     int Bases,
                                     double AVG,
                                     double SLG,
                                     double OBP,
                                     double OPS)
    {
        public PlayerSheetDisplay(string description,
                                           int teams,
                                           int games,
                                           PlayerStatsDisplay totals) : this(description,
                                                                             teams, 
                                                                             games,
                                                                             totals.Name,
                                                                             totals.PA,
                                                                             totals.AB,
                                                                             totals.R,
                                                                             totals.Singles,
                                                                             totals.Doubles,
                                                                             totals.Triples,
                                                                             totals.HR,
                                                                             totals.BB,
                                                                             totals.SF,
                                                                             totals.Hits,
                                                                             totals.Bases,
                                                                             totals.AVG,
                                                                             totals.SLG,
                                                                             totals.OBP,
                                                                             totals.OPS)
        {
        }
                                    
    }
}
