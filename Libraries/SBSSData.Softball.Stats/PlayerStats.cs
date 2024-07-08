using Newtonsoft.Json;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// Inherits a <see cref="Player"/> class and includes additional properties, for example, calculated
    /// stats.
    /// </summary>
    /// <remarks>
    /// Using inheritance allows this class to include all the properties of the base class. This class is used to encapsulate
    /// the data for a single player and single game, or a single player from multiple games (summary player data, 
    /// <see cref="TeamStats"/>), 
    /// or multiple players for a single game (game data, <see cref="GameTeamStats"/>), or even multiple players for multiple games 
    /// (team data, <see cref="TeamSummaryStats"/>).
    /// </remarks>
    public class PlayerStats : Player
    {
        /// <summary>
        /// Creates a instance using the data from the <paramref name="player"/> parameter, that is the 
        /// base object.
        /// </summary>
        /// <param name="player">A non-null <see cref="Player"/> instance.</param>
        /// <param name="numGames">The number of games that the data of this instance is recovered. This parameter is
        /// optional and default value is 1. This is always the case if the data is for a single player and single game (this is,
        /// not summary data).</param>
        /// <remarks>
        /// If <paramref name="player"/> is <c>null</c> or the <see cref="Player.Empty"/> instance, an
        /// "empty" (all properties have default values) instance is created.
        /// </remarks>
        public PlayerStats(Player player, int numGames = 1) : base(player)
        {
            NumGames = numGames;
        }

        /// <summary>
        /// Gets and sets the number of games from which stats are recovered; The default value is 1.
        /// </summary>
        [JsonIgnore]
        public int NumGames
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the number of plate appearances (not just official bats, but at bats plus bases on balls and sacrifice flies";
        /// </summary>
        [JsonIgnore]
        public int PlateAppearances => AtBats + BasesOnBalls + SacrificeFlies;

        /// <summary>
        /// Gets the first name of the <see cref="Player.Name"/> property. The <c>Player</c> name is of the form 
        /// "[last name],[first-name]". If the first name is not specified, the empty string is returned.
        /// </summary>
        [JsonIgnore]
        public string FirstName => (Name.Split(',').Length == 2) ? Name.Split(',')[1].Trim() : string.Empty;

        /// <summary>
        /// Gets the last name of the <see cref="Player.Name"/> property. The <c>Player</c> name is of the form 
        /// "[last-name],[first-name]". If the last name is not specified, "Unknown" is returned.
        /// </summary>
        [JsonIgnore]
        public string LastName => Name.Split(',')[0].Trim();

        /// <summary>
        /// Gets the display using the <see cref="FirstName"/> and <see cref="LastName"/> properties. The <c>Player</c> name is of the form 
        /// "[last-name],[first-name]". If the last name is not specified, "Unknown" is returned; otherwise a string of the
        /// format "[first-name] [last-name]" is returned.
        /// </summary>
        [JsonIgnore]
        public string DisplayName => (string.IsNullOrEmpty(LastName) ? FirstName : $"{FirstName} {LastName}").Trim();

        /// <summary>
        /// Returns the calculated number of total hits for the player, that is, 
        /// <code language="cs">
        /// TotalHits = Player.Singles + Player.Doubles + Player.Triples + Player.HomeRuns
        /// </code>
        /// </summary>
        [JsonIgnore]
        public int TotalHits => Singles + Doubles + Triples + HomeRuns;

        /// <summary>
        /// Returns the calculated number of total bases for the player, that is
        /// <code language="cs">
        /// TotalBases = Player.Singles + (2 * Player.Doubles) + (3 * Player.Triples) + (4 * Player.HomeRuns)
        /// </code>
        /// </summary>
        [JsonIgnore]
        public int TotalBases => Singles + (2 * Doubles) + (3 * Triples) + (4 * HomeRuns);

        /// <summary>
        /// Returns the calculated batting average for the player, that is
        /// <code language="cs">
        /// Average = Math.Round((double)TotalHits / (double)Player.AtBats, 3)
        /// </code>
        /// </summary>
        /// <remarks>
        /// If the number of at bats is zero, 0.0 is returned.
        /// </remarks>
        [JsonIgnore]
        public double Average
        {
            get
            {
                double ave = Math.Round((double)TotalHits / (double)AtBats, 3);
                return double.IsNaN(ave) ? 0 : ave;
            }
        }

        /// <summary>
        /// Returns the calculated slugging percentage for the player, that is
        /// <code language="cs">
        /// Slugging = Math.Round((double)TotalBases / (double)Player.AtBats, 3)
        /// </code>
        /// </summary>
        /// <remarks>
        /// If the number of at bats is zero, 0.0 is returned.
        /// </remarks>
        [JsonIgnore]
        public double Slugging
        {
            get
            {
                double slugging = Math.Round((double)TotalBases / (double)AtBats, 3);
                return double.IsNaN(slugging) ? 0 : slugging;
            }
        }

        /// <summary>
        /// Returns the calculated on base average for the player, that is
        /// <code language="cs">
        /// OnBase = Math.Round((double)(TotalHits + Player.BasesOnBalls) / (double)(Player.AtBats + Player.BasesOnBalls + Player.SacrificeFlies), 3)
        /// </code>
        /// </summary>
        /// <remarks>
        /// If the sum of the at bats, bases on balls and sacrifice flies is zero, 0.0 is returned.
        /// </remarks>
        [JsonIgnore]
        public double OnBase
        {
            get
            {
                double onBase = Math.Round((double)(TotalHits + BasesOnBalls) / (double)(AtBats + BasesOnBalls + SacrificeFlies), 3);
                return double.IsNaN(onBase) ? 0 : onBase;
            }
        }

        /// <summary>
        /// Returns the calculated on base average plus slugging percentage for the player, that is
        /// <code language="cs">
        /// OnBasePlusSlugging => Math.Round(OnBase + Slugging, 3);
        /// </code>
        /// </summary>
        [JsonIgnore]
        public double OnBasePlusSlugging => Math.Round(OnBase + Slugging, 3);

        /// <summary>
        /// Overrides the default to provide an identifying string for this instance.
        /// </summary>
        /// <returns>The value of the <see cref="DisplayName"/> property</returns>
        public override string ToString()
        {
            return DisplayName;
        }

    }
}
