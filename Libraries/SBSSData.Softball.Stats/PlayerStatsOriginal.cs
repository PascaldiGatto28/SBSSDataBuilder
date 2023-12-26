using Newtonsoft.Json;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// Encapsulates a <see cref="Player"/> instance and includes additional properties, for example, calculated
    /// stats.
    /// </summary>
    /// <remarks>
    /// Because all the addition properties are calculated from the <c>Player</c> instance, those properties are not
    /// serialized.
    /// </remarks>
    public sealed class PlayerStats
    {
        /// <summary>
        /// Creates a new empty instance where the contained <see cref="Player"/> is an "empty" instance.
        /// </summary>
        private PlayerStats()
        {
            // This just suppresses the warning that the property must be initialized.
            Player = Player.ConstructPlayer(Enumerable.Empty<PlayerLabelValue>());
        }

        /// <summary>
        /// Creates a instance using the data from the <paramref name="player"/> parameter.
        /// </summary>
        /// <param name="player">A non-null <see cref="Player"/> instance.</param>
        /// <exception cref="ArgumentNullException">if <paramref name="player"/> is <c>null</c></exception>
        public PlayerStats(Player player) : this()
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        /// <summary>
        /// Gets and initializes the contained <see cref="Player"/> property used to produce the addition properties.
        /// </summary>
        public Player Player
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the first name of the <see cref="Player.Name"/> property. The <c>Player</c> name is of the form 
        /// "[lastname],[firstname]". If the first name is not specified, the empty string is returned.
        /// </summary>
        [JsonIgnore]
        public string FirstName => (Player.Name.Split(',').Length == 2) ? Player.Name.Split(',')[1].Trim() : string.Empty;

        /// <summary>
        /// Gets the last name of the <see cref="Player.Name"/> property. The <c>Player</c> name is of the form 
        /// "[lastname],[firstname]". If the last name is not specified, "Unknown" is returned.
        /// </summary>
        [JsonIgnore]
        public string LastName => Player.Name.Split(',')[0].Trim();

        /// <summary>
        /// Gets the display using the <see cref="FirstName"/> and <see cref="LastName"/> properties. The <c>Player</c> name is of the form 
        /// "[lastname],[firstname]". If the last name is not specified, "Unknown" is returned; otherwise a string of the
        /// format "[firstname] [lastname]" is returned.
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
        public int TotalHits => Player.Singles + Player.Doubles + Player.Triples + Player.HomeRuns;

        /// <summary>
        /// Returns the calculated number of total bases for the player, that is
        /// <code language="cs">
        /// TotalBases = Player.Singles + (2 * Player.Doubles) + (3 * Player.Triples) + (4 * Player.HomeRuns)
        /// </code>
        /// </summary>
        [JsonIgnore]
        public int TotalBases => Player.Singles + (2 * Player.Doubles) + (3 * Player.Triples) + (4 * Player.HomeRuns);

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
                double ave = Math.Round((double)TotalHits / (double)Player.AtBats, 3);
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
                double slugging = Math.Round((double)TotalBases / (double)Player.AtBats, 3);
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
                double onBase = Math.Round((double)(TotalHits + Player.BasesOnBalls) / (double)(Player.AtBats + Player.BasesOnBalls + Player.SacrificeFlies), 3);
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
    }
}

