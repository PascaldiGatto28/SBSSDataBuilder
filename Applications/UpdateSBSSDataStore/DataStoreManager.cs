using Levaro.SBSoftball;
using Levaro.SBSoftball.Common;
using Levaro.SBSoftball.Logging;

namespace Levaro.Application.SBSSDataStore
{
    public sealed class DataStoreManager
    {
        private static readonly string DataStorePath = AppContext.Instance.Settings.DataStorePath;
        private static readonly Log log = AppContext.Instance.Log;

        public static void Run(bool update = true)
        {
            DataStoreManager manager = new();

            LeaguesData dataStore = DataStorePath.Deserialize<LeaguesData>();
            if (dataStore != null)
            {
                List<ScheduledGame> scheduledGames = dataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).ToList();
                int count = scheduledGames.Count;
                int complete = scheduledGames.Count(g => g.IsComplete);
                int cancelled = scheduledGames.Count(g => g.WasCancelled);
                log.WriteLine($"There are {count} scheduled games, of which {complete} are completed, and of those, {cancelled} were cancelled " +
                              $"as of {dataStore.BuildDate:dddd MMMM d, yyyy a\\t h:mm tt}");

                if (update)
                {
                    int updated = DataStoreManager.Update(scheduledGames);
                    if (updated > 0)
                    {
                        dataStore.BuildDate = DateTime.Now;
                        log.WriteLine($"{updated} games have been updated");
                        dataStore.Serialize(DataStorePath);
                        log.WriteLine($"The data has been serialized to {DataStorePath}");

                    }
                    else
                    {
                        log.WriteLine("The data store is up-to-date; no games were found.");
                    }
                }
                else
                {
                    log.WriteLine(LogCategory.Warning, "Rebuilding the data store is not available at this time.");
                }


            }
            else
            {
                log.WriteLine($"Unable to deserialize the LeaguesData object from \"{DataStorePath}\"");
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
                    log.WriteLine($"Processing {scheduledGame.GameResults.GameInformation}");
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
                    }

                    updated++;
                }
            }

            return updated;
        }
    }
}