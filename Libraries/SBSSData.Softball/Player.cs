using System.Reflection;
using System.Text.Json.Serialization;

using SBSSData.Softball.Common;

namespace SBSSData.Softball
{
    /// <summary>
    /// Encapsulates player statistics for a single game.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Creates an instance of this class.
        /// </summary>
        /// <remarks>
        /// Because all the properties of "init" setters, the <see cref="Player.ConstructPlayer"/>
        /// static method is really (short of reflection) was to create an new instance.
        /// </remarks>
        public Player()
        {
            Name = "Unknown";
        }

        /// <summary>
        /// Creates a copy of the specified instance (that is, a copy constructor).
        /// </summary>
        /// <param name="player">The existing <see cref="Player"/> whose property values are used to
        /// produce a new instance having all the same properties.</param>
        public Player(Player player) : this()
        {
            if (player != null)
            {
                Type type = typeof(Player);
                PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                foreach (PropertyInfo property in properties)
                {
                    object? value = property.GetValue(player, null);
                    type.GetProperty(property.Name)?.SetValue(this, value, null);
                }
            }
        }


        /// <summary>
        /// Constructs a instance with all properties initialized.
        /// </summary>
        /// <param name="labelValues">A sequence of <see cref="PlayerLabelValue"/> objects that provide a mapping between
        /// player game stat descriptions and properties of this class.</param>
        /// <returns>A populated instance of the <see cref="Player"/> class.</returns>
        /// <remarks>
        /// This method (short of reflection) is the only the way to construct an instance of this class that have
        /// populated properties. Moreover, it is only invoked from the 
        /// <see cref="Game.ConstructTeams(HtmlAgilityPack.HtmlDocument)"/> static method
        /// </remarks>
        public static Player ConstructPlayer(IEnumerable<PlayerLabelValue> labelValues)
        {
            Player player = new();
            Type playerType = typeof(Player);
            foreach (PlayerLabelValue labelValue in labelValues)
            {
                switch (labelValue.Label)
                {
                    case "Player":
                    {
                        PropertyInfo? property = playerType.GetProperty("Name");
                        property?.SetValue(player, labelValue.Value.CleanNameText());
                        break;
                    }
                    case "AB":
                    {
                        PropertyInfo? property = playerType.GetProperty("AtBats");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    case "R":
                    {
                        PropertyInfo? property = playerType.GetProperty("Runs");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    case "1B":
                    {
                        PropertyInfo? property = playerType.GetProperty("Singles");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    case "2B":
                    {
                        PropertyInfo? property = playerType.GetProperty("Doubles");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    case "3B":
                    {
                        PropertyInfo? property = playerType.GetProperty("Triples");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    case "HR":
                    {
                        PropertyInfo? property = playerType.GetProperty("HomeRuns");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    case "BB":
                    {
                        PropertyInfo? property = playerType.GetProperty("BasesOnBalls");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    case "SF":
                    {
                        PropertyInfo? property = playerType.GetProperty("SacrificeFlies");
                        property?.SetValue(player, labelValue.Value.ParseInt() ?? 0);
                        break;
                    }
                    default:
                    {
                        break;
                    }
                }
            }

            return player;
        }

        /// <summary>
        /// Returns an "empty" player object, that is, one with properties set to default values.
        /// </summary>
        /// <remarks>
        /// An empty player is just used as an initialized <see cref="Player"/> object and because the properties can only be
        /// initialized during construction, it has no use otherwise.
        /// </remarks>
        /// <seealso cref="Player.ConstructPlayer(IEnumerable{PlayerLabelValue})"/>
        [JsonIgnore]
        public static Player Empty => Player.ConstructPlayer(Enumerable.Empty<PlayerLabelValue>());

        /// <summary>
        /// Gets and initializes the player name.
        /// </summary>
        /// <remarks>
        /// The name is a text string of the format "[Lastname], [Firstname]".
        /// </remarks>
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
    }
}