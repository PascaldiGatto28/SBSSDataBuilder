using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Logging;
using SBSSData.Softball.Stats;

using AppContext = SBSSData.Application.Infrastructure.AppContext;


namespace SBSSData.Application.DataStore
{
    /// <summary>
    /// The essential code for creating and updating the data store.
    /// </summary>
    /// <remarks>
    /// An instance of the class is created by the console application entry point <see cref="Program.Main"/> and then the
    /// <see cref="DataStoreManager.Run(bool)"/> method is called which determines whether file data store is created or
    /// updated.
    /// </remarks>
    public sealed class DataStoreManager
    {
        private static readonly string DataStorePath = AppContext.Instance.Settings.DataStorePath;
        private static readonly Log log = AppContext.Instance.Log;

        /// <summary>
        /// Executes the code to either create or update the data store.
        /// </summary>
        /// <remarks>
        /// An instance of a <see cref="DataStoreContainer"/> is created based up the 
        /// <see cref="SBSSData.Application.Infrastructure.AppSettings.DataStorePath"/>
        /// value and has then a reference to the current data store. If a data store is altered or created, the 
        /// <see cref="DataStoreContainer.Save(string?, bool, string)"/> method is called to serialize the data to the data store
        /// JSON file.
        /// </remarks>
        /// <param name="update">If true, and an existing data store exists, it is updated with games that have been played since 
        /// the last update. If false the data store is created unless there already exists a data store in which it is updated,
        /// </param>
        /// <returns>
        /// A value indicating whether the data store has changed: <c>true</c> if it's been updated or created; <c>false</c>
        /// otherwise.
        /// </returns>
        public static bool Run(bool update)
        {
            bool modified = false;
            DataStoreManager manager = new();
            DataStoreContainer dsContainer = DataStoreContainer.Instance(DataStorePath);
            LeaguesData dataStore = dsContainer.DataStore;
            bool updating = update || dataStore.LeagueSchedules.Any();
            int bytesWritten = 0;

            if (updating)
            {
                List<ScheduledGame> scheduledGames = dataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).ToList();
                //log.WriteLine($"There are {dsContainer.NumberOfGames} scheduled games, of which {dsContainer.GamesRecorded} " +
                //              $"are completed, and of those, {dsContainer.GamesCanceled} were canceled " +
                //              $"as of {dataStore.BuildDate:dddd MMMM d, yyyy a\\t h:mm tt}");
                log.WriteLine($"The recovered data store:\r\n{dsContainer}");

                int updated = DataStoreManager.Update(scheduledGames);
                if (updated > 0)
                {
                    string numGames = updated == 1 ? "1 game" : $"{updated} games";
                    log.WriteLine($"{numGames} updated");
                    bytesWritten = dsContainer.Save();
                    log.WriteLine($"The data saved to {DataStorePath}; {bytesWritten.FormatInt(extend:true)} bytes written.");
                    modified = true;
                }
                else
                {
                    log.WriteLine("The data store is up-to-date; no games were found.");
                }
            }
            else
            {
                string dataStorePath = dsContainer.DataStorePath;
                if (dataStore.LeagueSchedules.Any())
                {
                    log.WriteLine(LogCategory.Warning, "The existing data store will be overwritten");
                }
                else
                {
                    log.WriteLine($"Building the data store at {dataStorePath}");
                }

                dsContainer = DataStoreManager.Build(dataStorePath);
                bytesWritten = dsContainer.Save();
                modified = true;
                log.WriteLine($"New data store constructed {bytesWritten:#,###} bytes written, and the created container:\r\n{dsContainer}");
            }

            return modified;
        }

        /// <summary>
        /// Updates an existing data store with new game information
        /// </summary>
        /// <param name="scheduledGames">
        /// The sequence of <see cref="ScheduledGame"/> which are checked see if they are completed and recorded. If so
        /// the information is gathered for SSSA Web site and the game information recorded in the data store.
        /// </param>
        /// <returns>The number of games that have been updated; 0 is none have been altered.</returns>
        public static int Update(IEnumerable<ScheduledGame> scheduledGames)
        {
            int updated = 0;

            // Scheduled games are never null because all exist when the data store is created.
            foreach (ScheduledGame scheduledGame in scheduledGames.Where(s => !s.IsComplete))
            {
                // A completed game is never null
                if (scheduledGame.IsRecorded)
                {
                    // When the data store is built, the GameResults property is never null. 

                    log.WriteLine($"Updating {scheduledGame.GameResults.GameInformation}");

                    Game currentGame = scheduledGame.GameResults;
                    Game updatedGame = Game.ConstructGame(scheduledGame, update: true);
                    scheduledGame.GameResults = updatedGame;
                    if (updatedGame.Teams.Count == 2)
                    {
                        scheduledGame.VisitorScore = updatedGame.Teams[0].RunsScored;
                        scheduledGame.HomeScore = updatedGame.Teams[1].RunsScored;
                    }
                    else
                    {
                        // For now on it will show completed so it won't be processed again.
                        // When the game date is less than the build date, and not complete the game has been canceled
                        // and will never be played.
                        scheduledGame.VisitorScore = 0;
                        scheduledGame.HomeScore = 0;
                        log.WriteLine($"Marked as canceled!");
                    }

                    updated++;
                }
            }

            return updated;
        }

        /// <summary>
        /// Creates a new <see cref="LeaguesData"/> data store.
        /// </summary>
        /// <param name="dataStorePath">The location where the data store should be saved.</param>
        /// <returns>The created <see cref="DataStoreContainer"/></returns>
        public static DataStoreContainer Build(string dataStorePath)
        {
            LeaguesData dataStore = LeaguesData.ConstructLeaguesData(message: (s) => log.WriteLine(s));
            DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath);
            dsContainer.Dispose();
            dsContainer = DataStoreContainer.Instance(dataStorePath, dataStore);
            return dsContainer;
        }
    }
}