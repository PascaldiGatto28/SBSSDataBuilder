using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Creates a record that encapsulates and orders selected properties of the <see cref="PlayerStats"/> class. This is used for 
    /// displaying only. The primary constructor is typically not called directly, but instead use the constructor taking a
    /// <see cref="PlayerStats"/> instance whose values are used to populate the record.
    /// </summary>
    /// <param name="Games">The number of games played by the player.</param>
    /// <param name="Name">The display name of the player.</param>
    /// <param name="PA">The number of plate appearances: official at bats plus bases on balls plus sacrifice flies.</param>
    /// <param name="AB">The number of official at bats.</param>
    /// <param name="R">The number of runs scored.</param>
    /// <param name="Singles">The number one-base (singles hits.</param>
    /// <param name="Doubles">The number of two-base (double) hits.</param>
    /// <param name="Triples">The number of three-base (triple) hits.</param>
    /// <param name="HR">The number of home runs.</param>
    /// <param name="BB">The number of bases on balls.</param>
    /// <param name="SF">The number of sacrifice flies.</param>
    /// <param name="TH">The total number of hits; <see cref="PlayerStats.TotalHits"/> for details..</param>
    /// <param name="TB">The total number of bases from hits, see <see cref="PlayerStats.TotalBases"/> for details.
    /// </param>
    /// <param name="Avg">The ratio of <paramref name="Hits"/> and official at bats. 
    /// See <see cref="PlayerStats.Average"/> for details.</param>
    /// <param name="Slug"><see cref="PlayerStats.Slugging"/> for details.</param>
    /// <param name="OBP"><see cref="PlayerStats.OnBase"/> for details.</param>
    /// <param name="OPS"><see cref="PlayerStats.OnBasePlusSlugging"/> for details.</param>
    public record PlayerStatsDisplay(int Games,
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
                                     double OPS)
    {

        /// <summary>
        /// Creates a record using the information the instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> object whose values are used to initialize the 
        /// <see cref="PlayerStatsDisplay"/> record.</param>
        public PlayerStatsDisplay(PlayerStats player) : this(player.NumGames,
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
                                                             player.OnBasePlusSlugging)
        {
        }
    }
}
