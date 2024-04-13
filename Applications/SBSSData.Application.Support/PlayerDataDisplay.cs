using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Creates a record that encapsulates and orders selected properties of the <see cref="PlayerData"/> class. This is used for 
    /// displaying only. The primary constructor is typically not called directly, but instead use the constructor taking a
    /// <see cref="PlayerData"/> instance whose values are used to populate the record.
    /// </summary>
    /// <param name="Name">The display name of the player.</param>
    /// <param name="AB">The number of official at bats.</param>
    /// <param name="R">The number of runs scored.</param>
    /// <param name="Singles">The number one-base (singles hits.</param>
    /// <param name="Doubles">The number of two-base (double) hits.</param>
    /// <param name="Triples">The number of three-base (triple) hits.</param>
    /// <param name="HR">The number of home runs.</param>
    /// <param name="BB">The number of bases on balls.</param>
    /// <param name="SF">The number of sacrifice flies.</param>
    /// <param name="Hits">The total number of hits; <see cref="PlayerData.TotalHits"/> for details..</param>
    /// <param name="Bases">The total number of bases from hits, see <see cref="PlayerData.TotalBases"/> for details.
    /// </param>
    /// <param name="AVG">The ratio of <paramref name="Hits"/> and official at bats. 
    /// See <see cref="PlayerData.Average"/> for details.</param>
    /// <param name="SLG"><see cref="PlayerData.Slugging"/> for details.</param>
    /// <param name="OBP"><see cref="PlayerData.OnBase"/> for details.</param>
    /// <param name="OPS"><see cref="PlayerData.OnBasePlusSlugging"/> for details.</param>
    public record PlayerDataDisplay(string Name,
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

        /// <summary>
        /// Creates a record using the information the instance of the <see cref="Player"/> class.
        /// </summary>
        /// <param name="player">The <see cref="Player"/> object whose values are used to initialize the 
        /// <see cref="PlayerDataDisplay"/> record.</param>
        public PlayerDataDisplay(PlayerData player) : this(player.DisplayName,
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
