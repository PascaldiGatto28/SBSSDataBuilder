using System.Reflection;

#nullable disable
namespace SBSSData.Softball.Stats
{
    public static class StatsUtilities
    {

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

        public static T SumIntProperties<T>(this IEnumerable<T> data, T instance)
        {
            IEnumerable<PropertyInfo> properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                                            .Where(p => p.PropertyType == typeof(int));
            foreach (PropertyInfo property in properties)
            {
                property.SetValue(instance, data.Select(p => (int)property.GetValue(p)).ToList().Sum());
            }

            return instance;
        }

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
