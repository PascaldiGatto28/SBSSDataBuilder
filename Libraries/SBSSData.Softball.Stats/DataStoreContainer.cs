using System.Reflection;
using System.Text.Json;

using SBSSData.Softball.Common;

namespace SBSSData.Softball.Stats
{
    /// <summary>
    /// This class is a containing for the data store (<see cref="LeaguesData"/> object and has services to deserialize, 
    /// serialize, save and copy the JSON serialized data store. 
    /// </summary>
    /// <remarks>
    /// This is a singleton class providing services through its <see cref="DataStoreContainer.Instance(string, LeaguesData?)"/> static
    /// method.
    /// </remarks>
    public sealed class DataStoreContainer : IDisposable
    {
        /// <summary>
        /// The private static field that contains the one and only instance at a time of this class.
        /// </summary>
        private static DataStoreContainer? instance = null;

        /// <summary>
        /// Returns an instance with <see cref="DataStore"/> and <see cref="DataStorePath"/> set to <see cref="LeaguesData.Empty"/>
        /// and <see cref="String.Empty"/> respectively.
        /// </summary>
        private DataStoreContainer()
        {
            DataStore = LeaguesData.Empty;
            DataStorePath = string.Empty;
        }

        /// <summary>
        /// Returns an empty instance, that is one whose properties are the default values.
        /// </summary>
        /// <remarks>
        /// This is only valuable when needing to initialize a <see cref="DataStoreContainer"/> to a non-null value, for 
        /// example when it is used as a property. 
        /// </remarks>
        /// <seealso cref="Query"/>
        public static DataStoreContainer Empty => new();


        /// <summary>
        /// The private constructor call by the <see cref="instance"/> method that creates the object.
        /// </summary>
        /// <param name="path">A fully qualified path to a JSON serialization of the <see cref="LeaguesData"/> object. The
        /// object can deserialized and can be accessed from the <see cref="DataStore"/> property.
        /// Even if the property is not used to initialize <c>DataStore</c>, it must be specified and makes it possible to
        /// use the default version of the <see cref="Save(string?, bool, string)"/> method.</param>
        /// <param name="leaguesData">
        /// An optional <see cref="LeaguesData"/> object. If not specified (the default is <c>null</c>), a
        /// <c>LeaguesData</c> object is deserialized from the file specified by the <paramref name="path"/> parameter.
        /// </param>
        private DataStoreContainer(string path, LeaguesData? leaguesData = null)
        {
            DataStore = leaguesData ?? GetDataStore(path);
            DataStorePath = path;
        }

        /// <summary>
        /// Returns an instance of this class where the data specified the <paramref name="path"/> is a JSON serialization of
        /// the <see cref="LeaguesData"/> object, or if an <c>LeaguesData</c> is optionally provided.
        /// </summary>
        /// <remarks>
        /// This the only way to create and instance of this class. Once created, successive calls returns the same object.
        /// The <see cref="Reset"/> method can be used to force a new instance to be created.
        /// </remarks>
        /// <param name="path">The fully qualified path to the serialized instance of a <see cref="LeaguesData"/> object</param>
        /// <param name="leaguesData">
        /// An optional <see cref="LeaguesData"/> object. If not specified (the default is <c>null</c>), a
        /// <c>LeaguesData</c> object is deserialized from the file specified by the <paramref name="path"/> parameter.
        /// </param>
        /// <returns>A <see cref="DataStoreContainer"/> object.</returns>
        public static DataStoreContainer Instance(string path, LeaguesData? leaguesData = null)
        {
            instance ??= new DataStoreContainer(path, leaguesData);
            return instance;
        }

        /// <summary>
        /// Gets the current <see cref="LeaguesData"/> deserialized object.
        /// </summary>
        /// <remarks>
        /// Properties of this class are accessed via the constructed instance, for example
        /// <code language="cs">
        /// DataStoreContainer dsContainer = DataStoreContainer.Instance;
        /// LeaguesData dataStore = dsContainer.DataStore;
        /// </code>"
        /// </remarks>
        public LeaguesData DataStore
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path to the <see cref="LeaguesData"/> serialized object. 
        /// </summary>
        /// <remarks>
        /// Properties of this class are accessed via the constructed instance, for example
        /// <code language="cs">
        /// DataStoreContainer dsContainer = DataStoreContainer.Instance;
        /// string dataStorePath = dsContainer.DataStorePath;
        /// </code>"
        /// </remarks>
        public string DataStorePath
        {
            get;
            private set;
        }

        /// <summary>
        /// The list of all scheduled games in all leagues.
        /// </summary>
        /// <remarks>
        /// </remarks>
        public List<ScheduledGame> Games => DataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).ToList();

        /// <summary>
        /// Gets the total number of scheduled games in all leagues.
        /// </summary>
        /// <remarks>
        /// Once leagues are scheduled and the data store built, this number should not change.
        /// </remarks>
        public int NumberOfGames => Games.Count;

        /// <summary>
        /// Gets the number of scheduled games in all leagues that have been completed, that is no longer waiting to be played.
        /// </summary>
        public int GamesCompleted => Games.Count(g => g.IsComplete);

        /// <summary>
        /// Gets the number of scheduled games in all leagues that have been canceled.
        /// </summary>
        /// <remarks>
        /// Although canceled games have not been played, that is there are no player stats for the teams, they are considered
        /// completed.
        /// </remarks>
        public int GamesCanceled => Games.Count(g => g.WasCancelled);

        /// <summary>
        /// The number of games actually played in all leagues, that is games where there are player stats for the teams.
        /// </summary>
        public int GamesPlayed => Games.Count(g => g.IsComplete && !g.WasCancelled);

        /// <summary>
        /// Saves the current <see cref="DataStore"/> by serializing it to the file specified <paramref name="path"/> 
        /// property value.
        /// </summary>
        /// <param name="path">The full qualified path specify where the JSON serialization results are placed. This
        /// is an optional parameter and if not specified, the <see cref="DataStorePath"/> property value is used and
        /// can overwrite the current JSON file.
        /// </param>
        /// <param name="backup">This parameter is optional and the default value is <c>true</c>. If <c>true</c> and you
        /// choose to write over the existing serialized file, a backup of the current file before overwritten is saved.
        /// If the <c>false</c> or the value of <paramref name="path"/> is the value of the <c>DataStorePath</c> property
        /// no action is taken.
        /// </param>
        /// <param name="backupFolder">The subfolder of the <paramref name="path"/> where the backup file is written. This
        /// is optional and only used if a backup is requested. The default value is "Backup". If the folder needs to
        /// be created, it is the immediate parent of the file itself. For example, if the <c>DataStorePath</c> is
        /// <c>D:\Temp\SBSS\LeaguesData.json</c>, the default backup path is 
        /// <c>D:\Temp\SBSS\Backup\LeaguesData-[time stamp].json</c>, where [time stamp] is a string of the form
        /// <c>MM-dd-yyyy-HH-mm-ss.ff</c> (ff is hundredths of a second). 
        /// </param>
        /// <remarks>
        /// If you invoke this method with just the defaults, the data store is serialized and overwrites the current file
        /// after it is saved to a the "Create" subfolder with the same name and extension as the original but having a 
        /// time stamp.
        /// </remarks>
        /// <returns>The number of bytes written to the serialization path. If an error occurs, 0 is returned.</returns>
        public int Save(string? path = null, bool backup = true, string backupFolder = "Backup")
        {
            int bytesSaved = 0;

            string dataStorePath = string.IsNullOrEmpty(path) ? DataStorePath : path;
            if (!string.Equals(dataStorePath, DataStorePath, StringComparison.OrdinalIgnoreCase))
            {
                if (Path.IsPathFullyQualified(dataStorePath))
                {
                    string folder = Path.GetDirectoryName(dataStorePath) ?? string.Empty;
                    Directory.CreateDirectory(folder);
                    DataStore.BuildDate = DateTime.Now;
                    bytesSaved = DataStore.Serialize(dataStorePath);
                    if (bytesSaved > 0)
                    {
                        DataStorePath = dataStorePath;
                    }
                }
            }
            else
            {
                // If the dataStorePath is the same as DataStorePath, then may need to create a backup folder
                // and backup the currently existing dataStore before saving this current dataStore.

                if (backup && File.Exists(dataStorePath))
                {
                    string name = new FileInfo(dataStorePath).Name;
                    string folder = Path.GetDirectoryName(dataStorePath) ?? string.Empty;
                    string newFolder = $@"{folder}\{backupFolder}";
                    Directory.CreateDirectory(newFolder);
                    string newPath = $@"{newFolder}\{name}";

                    // Finally add a time stamp to the file name and then copy the file to the backup folder.
                    newPath = newPath.AppendTextToFileName($"{DataStore.BuildDate:-MM-dd-yyyy HH-mm-ss.ff}");

                    File.Copy(dataStorePath, newPath);
                }

                DataStore.BuildDate = DateTime.Now;
                bytesSaved = DataStore.Serialize(dataStorePath);
            }

            return bytesSaved;
        }

        /// <summary>
        /// Forces the next call to the <see cref="Instance(string,LeaguesData?)"/> to deserialize the data store from the 
        /// <see cref="DataStorePath"/>,
        /// </summary>
        public void Reset()
        {
            instance = new DataStoreContainer(DataStorePath);
        }

        /// <summary>
        /// Deserialize <see cref="LeaguesData"/> from the specified file.
        /// </summary>
        /// <remarks>
        /// A number of checks are made to make sure the <paramref name="dataStorePath"/> is valid, and if not or the file
        /// can be deserialized, exceptions are thrown. This method is called by the private constructor 
        /// <see cref="DataStoreContainer(string, LeaguesData?)"/> when populating the <see cref="DataStore"/> property.
        /// </remarks>
        /// <param name="dataStorePath">The fully qualified path to the JSON file which is deserialized to populate
        /// the <c>DataStore</c> property.
        /// </param>
        /// <returns>
        /// A <see cref="LeaguesData"/> object; it may be <see cref="LeaguesData.Empty"/> but is never <c>null</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">If the database file is not valid and can not be deserialized to
        /// a <see cref="LeaguesData"/> object.</exception>
        /// <exception cref="FileNotFoundException">If data store path is either null or the file cannot be found.</exception>
        private static LeaguesData GetDataStore(string dataStorePath)
        {
            LeaguesData data = LeaguesData.Empty;
            if (!string.IsNullOrEmpty(dataStorePath) && File.Exists(dataStorePath))
            {
                try
                {
                    data = dataStorePath.Deserialize<LeaguesData>() ?? LeaguesData.Empty;
                }
                catch (JsonException exception)
                {
                    throw new InvalidOperationException("The SBSS database file is not valid. Is the JSON text correct?", exception);
                }
            }
            //else
            //{
            //    //throw new FileNotFoundException("The data store path is either null or the file cannot be found.");
            //    data = 
            //}

            return data;
        }

        /// <summary>
        /// Overrides the default <see cref="object.ToString()"/> method to provide summary descriptions and values for each of the
        /// properties.
        /// </summary>
        /// <remarks>
        /// This method is most useful when writing information to the logging system.
        /// </remarks>
        /// <returns>
        /// A string that represents the current instance. For example,
        /// <code language="cs">
        /// Data Store : Build Date 12/11/2023 11:19:04 AM; Number of Leagues 9
        /// Data Store Path : D:\Softball\WorkingStorage\LeaguesData-Fall2023.json
        /// Number of Games : 154
        /// Games Completed : 138
        /// Games Canceled : 5
        /// Games Played : 133
        /// </code>
        /// </returns>
        public override string ToString()
        {
            PropertyInfo[] properties = typeof(DataStoreContainer).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            string text = properties.Where(p => p.Name != "Games").ToString<PropertyInfo>(x =>
            {
                return $"{(x.Name).NameToTitle()} : {x.GetValue(this)}";
            }, "\r\n");

            return text;
        }

        /// <summary>
        /// Sets the <see cref="instance"/> field to <c>null</c> and the other properties to initial "empty" values.
        /// </summary>
        /// <remarks>
        /// This method is required to implement the <see cref="IDisposable"/> interface.
        /// </remarks>
        public void Dispose()
        {
            instance = null;
            DataStorePath = string.Empty;
            DataStore = LeaguesData.Empty;
        }
    }
}
