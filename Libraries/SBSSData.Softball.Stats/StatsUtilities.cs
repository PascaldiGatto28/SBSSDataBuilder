using System.Reflection;

#nullable disable
namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// Provides utility extension methods for working with player statistics.
    /// </summary>
    /// <remarks>
    /// Methods in this class are implemented as extension methods to operate on
    /// <see cref="Player"/> instances and sequences of player-like objects.
    /// </remarks>
    public static class StatsUtilities
    {
        /// <summary>
        /// Produces a copy of the specified <see cref="Player"/> with its <see cref="Player.Name"/> changed.
        /// </summary>
        /// <param name="player">The source player to copy. If null, <see cref="Player.Empty"/> is used.</param>
        /// <param name="name">The new name to set on the copied player. If null or empty, the name is not changed.</param>
        /// <returns>
        /// A <see cref="Player"/> instance that is either a copy of the original with the updated name,
        /// or <see cref="Player.Empty"/> if the source player is null.
        /// </returns>
        public static Player ChangeName(this Player player, string name = null)
        {
            Player copiedPlayer = player ?? Player.Empty;
            if ((player != null) && !string.IsNullOrEmpty(name))
            {
                copiedPlayer = new Player(player);
                typeof(Player).GetProperty("Name")?.SetValue(copiedPlayer, name, null);
            }

            return copiedPlayer;
        }

        /// <summary>
        /// Sums all publicly writable integer properties on the supplied instance from the sequence of data.
        /// </summary>
        /// <typeparam name="T">The type of objects in <paramref name="data"/> and of <paramref name="instance"/>.</typeparam>
        /// <param name="data">A sequence of objects whose integer properties will be summed.</param>
        /// <param name="instance">An instance of <typeparamref name="T"/> whose writable integer properties will be set to the sums.</param>
        /// <returns>The same <paramref name="instance"/> after its integer properties have been set to the computed sums.</returns>
        /// <remarks>
        /// Only properties of type <see cref="int"/> that are public instance properties and writable are considered.
        /// Any property that cannot be read or summed is silently ignored.
        /// </remarks>
        public static T SumIntProperties<T>(this IEnumerable<T> data, T instance)
        {
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                            .Where(p => (p.PropertyType == typeof(int)) && p.CanWrite);
            foreach (PropertyInfo property in properties)
            {
                //try
                {
                    property.SetValue(instance, data.Select(p => (int)property.GetValue(p)).ToList().Sum());
                }
                //catch 
                { 
                    // Swallow if it can't be 
                }
            }

            return instance;
        }

        /// <summary>
        /// Creates a new instance of <typeparamref name="T"/> using its parameterless constructor and
        /// populates its public integer properties by summing corresponding properties from <paramref name="data"/>.
        /// </summary>
        /// <typeparam name="T">The type to instantiate and populate. Must expose a parameterless constructor.</typeparam>
        /// <param name="data">A sequence of objects whose integer properties will be summed into the new instance.</param>
        /// <returns>
        /// A new instance of <typeparamref name="T"/> whose public integer properties contain the sums.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a parameterless constructor cannot be found or an instance cannot be created.
        /// </exception>
        /// <remarks>
        /// All public instance properties of type <see cref="int"/> are considered. Null elements in <paramref name="data"/>
        /// are ignored when computing sums.
        /// </remarks>
        public static T SumIntProperties<T>(this IEnumerable<T> data)
        {
            T instance = default;
            Type type = typeof(T);
            ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                                                              Type.EmptyTypes);

            if (constructor != null)
            {
                instance = (T)(constructor.Invoke(Array.Empty<object>()));
            }
            else
            {
                throw new InvalidOperationException("The default constructor be found. The type must have a declared default constructor.");
            }

            if (instance != null)
            {
                IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                                .Where(p => p.PropertyType == typeof(int));
                foreach (PropertyInfo property in properties)
                {
                    property.SetValue(instance, data.Where(d => d != null)
                                                    .Select(d => (int)property.GetValue(d)).ToList().Sum());
                }
            }
            else
            {
                throw new InvalidOperationException("Unable to create an instance; is the default constructor valid?");
            }

            return instance;
        }

        /// <summary>
        /// Produces a summary <see cref="Player"/> by summing integer statistics across a sequence of players.
        /// </summary>
        /// <param name="data">The sequence of <see cref="Player"/> instances to summarize.</param>
        /// <param name="name">
        /// Optional name to assign to the resulting summary player. If not supplied and all players share the same name,
        /// that shared name is retained; otherwise the provided name is applied.
        /// </param>
        /// <returns>
        /// A <see cref="Player"/> instance representing the summed statistics across <paramref name="data"/>,
        /// or <see cref="Player.Empty"/> if <paramref name="data"/> is null or empty.
        /// </returns>
        public static Player PlayersSummary(this IEnumerable<Player> data, string name = null)
        {
            Player player = null;
            if ((data != null) && data.Any())
            {
                string firstPlayerName = data.First().Name;
                bool singlePlayer = data.Where(p => p.Name == firstPlayerName).Count() == data.Count();
                player = data.SumIntProperties() ?? Player.Empty;
                player = player?.ChangeName(!singlePlayer ? name : firstPlayerName);
            }

            return player ?? Player.Empty;
        }

        /// <summary>
        /// Attempts to read the value of a <see cref="PropertyInfo"/> from an instance and return it as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the property value to return.</typeparam>
        /// <param name="property">The reflection <see cref="PropertyInfo"/> describing the property to read.</param>
        /// <param name="instance">The object instance from which the property value will be retrieved.</param>
        /// <returns>
        /// The property value cast to <typeparamref name="T"/> if the value is non-null and of the expected type;
        /// otherwise the default value of <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>
        /// This method performs a runtime type check and will not attempt to convert incompatible types.
        /// If <paramref name="instance"/> is null or the property's value is null or of a different type, the default
        /// value for <typeparamref name="T"/> is returned.
        /// </remarks>
        public static T GetPropertyValue<T>(this PropertyInfo property, object instance)
        {
            T retValue = default;
            if (instance != null)
            {
                object value = property.GetValue(instance);
                if ((value != null) && (value.GetType() == typeof(T)))
                {
                    retValue = (T)value;
                }
            }

            return retValue;
        }
    }
}
