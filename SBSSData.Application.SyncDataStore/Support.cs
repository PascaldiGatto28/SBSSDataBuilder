using Dumpify;

using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.SyncDataStore
{
    /// <summary>
    /// Support static class for the SyncDataStore application
    /// </summary>
    public static class Support
    {
        /// <summary>
        /// Constructs the data store container using the current data from the SSSA website.
        /// </summary>
        /// <param name="constructedDSPath">The location of the constructed data store (JSON file) </param>
        /// <returns></returns>
        public static int ConstructCurrent(string constructedDSPath, bool build = false)
        {
            string builtDSPath = constructedDSPath;
            if (!File.Exists(StaticConstants.ConstructedDSPath) || build)
            {
                try
                {
                    LeaguesData dataStore = LeaguesData.ConstructLeaguesData(message: (s) => Console.WriteLine(s));
                    DataStoreContainer dsContainer = DataStoreContainer.Instance(builtDSPath, dataStore);
                    dsContainer.Save(builtDSPath);
                    dsContainer.Dispose();
                }
                catch (InvalidOperationException exception)
                {
                    exception.Dump();
                    throw;
                }
            }

            return File.ReadAllText(builtDSPath)?.Length ?? 0;
        }

        /// <summary>
        /// Returns all the games having data from the data store.
        /// </summary>
        /// <param name="dataStorePath">The path of the JSON that is the data which is queried for the actuve
        /// games (completed but not canceled or forfeited</param>
        /// <returns></returns>
        public static SortedList<string, Game> GetSortedGames(string dataStorePath)
        {
            SortedList<string, Game> sortedGames = [];
            LeaguesData lg = dataStorePath?.Deserialize<LeaguesData>() ?? LeaguesData.Empty;
            IEnumerable<ScheduledGame> scheduledGames = lg.LeagueSchedules.SelectMany(s => s.ScheduledGames);
            IEnumerable<Game> lgPlayedGames = scheduledGames.Where(s => s.IsComplete && !s.WasCanceled)
                                                                         .Select(s => s.GameResults)
                                                                         .Where(s => !s.IsForfeited)
                                                                         .OrderBy(s => s.GameInformation.GameId);
            lgPlayedGames.ToList().ForEach(g => sortedGames.Add(g.GameInformation.GameId, g));
            return sortedGames;
        }

        /// <summary>
        /// Compares the games from the two data stores.
        /// </summary>
        /// <param name="currentGames"></param>
        /// <param name="builtGames"></param>
        /// <returns>The IDs of the games that are not equal, the keys of the games in  the current in thar are not in
        /// the buuilt games and visa versa.</returns>
        public static CompareResults CompareGames(SortedList<string, Game> currentGames, SortedList<string, Game> builtGames)
        {
            List<string> unEqualKeys = [];
            List<string> builtKeysNotFound = [];
            List<string> currentKeysNotFound = [];

            foreach (string key in builtGames.Keys)
            {
                if (currentGames.TryGetValue(key, out Game? game))
                {
                    if (!builtGames[key].Equals(game))
                    {
                        unEqualKeys.Add(key);
                    }
                }
                else
                {
                    builtKeysNotFound.Add(key);
                }
            }

            foreach (string key in currentGames.Keys)
            {
                if (builtGames.TryGetValue(key, out Game? game))
                {
                    if (!currentGames[key].Equals(game) && !unEqualKeys.Contains(key))
                    {
                        unEqualKeys.Add(key);
                    }
                }
                else
                {
                    currentKeysNotFound.Add(key);
                }
            }

            return new CompareResults
            {
                UnequalGames = unEqualKeys,
                CurrentKeysNotFound = currentKeysNotFound,
                BuiltKeysNotFound = builtKeysNotFound
            };
        }
    }
}


