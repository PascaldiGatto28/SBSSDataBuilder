using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record PlayerStatistics(int Games,
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
                                   int TH,
                                   int TB,
                                   double AVG,
                                   double SLG,
                                   double OBP,
                                   double OPS,
                                   double AVGZScore,
                                   double SLGZScore,
                                   double OBPZScore,
                                   double OPSZScore)
    {

        public PlayerStatistics(PlayerStats player,
                                List<double> playerZScores) : this(player.NumGames,
                                                                   player.DisplayName,
                                                                   player.PlateAppearances,
                                                                   player.AtBats,
                                                                   player.Runs,
                                                                   player.Singles,
                                                                   player.Doubles,
                                                                   player.Triples,
                                                                   player.HomeRuns,
                                                                   player.BasesOnBalls,
                                                                   player.SacrificeFlies,
                                                                   player.TotalHits,
                                                                   player.TotalBases,
                                                                   player.Average,
                                                                   player.Slugging,
                                                                   player.OnBase,
                                                                   player.OnBasePlusSlugging,
                                                                   playerZScores[0],
                                                                   playerZScores[1],
                                                                   playerZScores[2],
                                                                   playerZScores[3])
        {
        }
    }
}
