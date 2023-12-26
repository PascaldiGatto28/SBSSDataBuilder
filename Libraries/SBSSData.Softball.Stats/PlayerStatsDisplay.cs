using System.Reflection;

namespace SBSSData.Softball.Stats
{
    public sealed class PlayerStatsDisplay
    {
        /// <summary>
        /// Creates a instance class where all properties are initialized to their default values.
        /// </summary>
        /// <remarks>
        /// This private constructor ensures that instances are only created using the public constructed and that serialized
        /// instance can be deserialized.
        /// </remarks>
        private PlayerStatsDisplay()
        {
            Name = "Unknown";
            FirstName = "Unknown";
            LastName = "Unknown";
            DisplayName = "Unknown";
        }

        /// <summary>
        /// Creates a instance of this class using the property values from the <paramref name="playerStats"/> parameter
        /// </summary>
        /// <param name="playerStats">
        /// A non-null <see cref="PlayerStats"/> object whose property values are used to initialize all the 
        /// properties of this class.
        /// </param>
        /// <exception cref="InvalidOperationException"> if either the <paramref name="playerStats"/> is <c>null</c>
        /// or its <see cref="PlayerStats.Player"/> property is <c>null</c>.</exception>
        public PlayerStatsDisplay(PlayerStats playerStats)
        {
            if ((playerStats != null) && (playerStats.Player != null))
            {
                IEnumerable<PropertyInfo> properties = typeof(PlayerStats).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                                          .Where(p => p.Name != "Player");
                Type displayType = typeof(PlayerStatsDisplay);
                foreach (PropertyInfo property in properties)
                {
                    var value = property.GetValue(playerStats, null);
                    displayType.GetProperty(property.Name)?.SetValue(this, value, null);
                }

                IEnumerable<PropertyInfo> playerProperties = typeof(Player).GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (PropertyInfo property in playerProperties)
                {
                    var value = property.GetValue(playerStats.Player, null);
                    displayType.GetProperty(property.Name)?.SetValue(this, value, null);
                }
            }
            else
            {
                throw new InvalidOperationException("playerStats and playerStats.Player must not be null",
                                                    new ArgumentNullException(nameof(playerStats)));
            }

        }

        #region Properties from Player
        /// <summary>
        /// Gets and initializes the name of the player. 
        /// </summary>
        public string Name
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of official at bats for the player. 
        /// </summary>
        public int AtBats
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of runs scored by the player.
        /// </summary>
        public int Runs
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of single base hits by the player.
        /// </summary>
        public int Singles
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of doubles by the player.
        /// </summary>
        public int Doubles
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of triples by the player.
        /// </summary>
        public int Triples
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of home runs by the player.
        /// </summary>
        public int HomeRuns
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number bases on balls (walks) given to the player for the game.
        /// </summary>
        public int BasesOnBalls
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the number of sacrifice flies hit by the player.
        /// </summary>
        public int SacrificeFlies
        {
            get;
            init;
        }
        #endregion

        /// <summary>
        /// Gets and initializes the first name of the <see cref="Player.Name"/> property. The <c>Player</c> name is of the form 
        /// "[lastname],[firstname]". If the first name is not specified, the empty string is returned.
        /// </summary>
        public string FirstName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the last name of the <see cref="Player.Name"/> property. The <c>Player</c> name is of the form 
        /// "[lastname],[firstname]". If the last name is not specified, "Unknown" is returned.
        /// </summary>
        public string LastName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the display using the <see cref="FirstName"/> and <see cref="LastName"/> properties. The <c>Player</c> name is of the form 
        /// "[lastname],[firstname]". If the last name is not specified, "Unknown" is returned; otherwise a string of the
        /// format "[firstname] [lastname]" is returned.
        /// </summary>
        public string DisplayName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the calculated number of total hits for the player, that is, 
        /// <code language="cs">
        /// TotalHits = Player.Singles + Player.Doubles + Player.Triples + Player.HomeRuns
        /// </code>
        /// </summary>
        public int TotalHits
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the calculated number of total bases for the player, that is
        /// <code language="cs">
        /// TotalBases = Player.Singles + (2 * Player.Doubles) + (3 * Player.Triples) + (4 * Player.HomeRuns)
        /// </code>
        /// </summary>
        public int TotalBases
        {
            get;
            init;
        }
        /// <summary>
        /// Gets and initializes the calculated batting average for the player, that is
        /// <code language="cs">
        /// Average = Math.Round((double)TotalHits / (double)Player.AtBats, 3)
        /// </code>
        /// </summary>
        /// <remarks>
        /// If the number of at bats is zero, 0.0 is returned.
        /// </remarks>
        public double Average
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the calculated slugging percentage for the player, that is
        /// <code language="cs">
        /// Slugging = Math.Round((double)TotalBases / (double)Player.AtBats, 3)
        /// </code>
        /// </summary>
        /// <remarks>
        /// If the number of at bats is zero, 0.0 is returned.
        /// </remarks>
        public double Slugging
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the calculated on base average for the player, that is
        /// <code language="cs">
        /// OnBase = Math.Round((double)(TotalHits + Player.BasesOnBalls) / (double)(Player.AtBats + Player.BasesOnBalls + Player.SacrificeFlies), 3)
        /// </code>
        /// </summary>
        /// <remarks>
        /// If the sum of the at bats, bases on balls and sacrifice flies is zero, 0.0 is returned.
        /// </remarks>
        public double OnBase
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the calculated on base average plus slugging percentage for the player, that is
        /// <code language="cs">
        /// OnBasePlusSlugging => Math.Round(OnBase + Slugging, 3);
        /// </code>
        /// </summary>
        public double OnBasePlusSlugging
        {
            get;
            init;
        }
    }
}
