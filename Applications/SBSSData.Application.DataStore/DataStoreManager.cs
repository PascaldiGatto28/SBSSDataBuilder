using SBSSData.Softball;
using SBSSData.Softball.Logging;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.DataStore
{
    public sealed class DataStoreManager
    {
        private static readonly string DataStorePath = AppContext.Instance.Settings.DataStorePath;
        private static readonly Log log = AppContext.Instance.Log;

        public static void Run(bool update)
        {
            DataStoreManager manager = new();
            DataStoreContainer dsContainer = DataStoreContainer.Instance(DataStorePath);
            LeaguesData dataStore = dsContainer.DataStore;
            bool updating = update || dataStore.LeagueSchedules.Any();

            if (updating)
            {
                List<ScheduledGame> scheduledGames = dataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).ToList();
                log.WriteLine($"There are {dsContainer.NumberOfGames} scheduled games, of which {dsContainer.GamesCompleted} " +
                              $"are completed, and of those, {dsContainer.GamesCanceled} were canceled " +
                              $"as of {dataStore.BuildDate:dddd MMMM d, yyyy a\\t h:mm tt}");

                int updated = DataStoreManager.Update(scheduledGames);
                if (updated > 0)
                {
                    log.WriteLine($"{updated} games have been updated");
                    int bytesWrittent = dsContainer.Save();
                    log.WriteLine($"The data has been serialized to {DataStorePath}; {bytesWrittent:#,###} bytes written.");

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
            }
        }

        public static int Update(IEnumerable<ScheduledGame> scheduledGames)
        {
            int updated = 0;
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
                        // When the game date is less than the build date, and not complete the game has been cancelled
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

        public static DataStoreContainer Build(string dataStorePath)
        {
            LeaguesData dataStore = LeaguesData.ConstructLeaguesData(message: (s) => log.WriteLine(s));
            DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath);
            dsContainer.Dispose();
            dsContainer = DataStoreContainer.Instance(dataStorePath, dataStore);
            dsContainer.Save();
            log.WriteLine($"New data store constructed and container created\r\n{dsContainer}");
            return dsContainer;
        }
    }
}