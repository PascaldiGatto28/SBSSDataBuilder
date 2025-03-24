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
        public static SortedList<string, Game> GetSortedGames(string dataStorePath, List<string>? excludedGames = null)
        {
            excludedGames ??= StaticConstants.ExcludedGames;
            SortedList<string, Game> sortedGames = [];
            LeaguesData lg = dataStorePath?.Deserialize<LeaguesData>() ?? LeaguesData.Empty;
            IEnumerable<ScheduledGame> scheduledGames = lg.LeagueSchedules.SelectMany(s => s.ScheduledGames);
            IEnumerable<Game> lgPlayedGames = scheduledGames.Where(s => s.IsComplete && !s.WasCanceled)
                                                                         .Select(s => s.GameResults)
                                                                         .Where(s => !s.IsForfeited && !excludedGames.Contains(s.GameInformation.GameId))
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
                //CurrentGames = currentGames,
                //BuiltGames = builtGames,
                UnequalGames = ShowTextIfEmpty(unEqualKeys,"No unequal keys"),
                CurrentKeysNotFound = ShowTextIfEmpty(currentKeysNotFound, "All current keys found"),
                BuiltKeysNotFound = ShowTextIfEmpty(builtKeysNotFound, "All built keys found")
            }; 
        }

        private static List<string> ShowTextIfEmpty (List<string> strings, string text)
        {
             return strings.Count == 0 ? [text] : strings;
        }   

        public static void ViewUnEqualGames(CompareResults results, SortedList<string, Game> currentGames, SortedList<string, Game> builtGames)
        {

            foreach (string key in results.UnequalGames)
            {
                Console.WriteLine($"Game {key} is not equal");
                bool equal = false;
                Game currentGame = currentGames[key];
                Game builtGame = builtGames[key];
                equal = currentGame.GameInformation.Equals(builtGame.GameInformation).Dump("Game Information");
                if (!equal)
                {
                    currentGame.GameInformation.Dump();
                    builtGame.GameInformation.Dump();
                }
                
                equal = currentGame.Teams[0].Equals(builtGame.Teams[0]).Dump("Visiting Teams");
                if (!equal)
                { 
                    List<Player> currentPlayers = currentGame.Teams[0].Players;
                    List<Player> builtPlayers = builtGame.Teams[0].Players;
                    for (int i = 0; i < currentPlayers.Count; i++)
                    {
                        if (!currentPlayers[i].Equals(builtPlayers[i]))
                        {
                            currentPlayers[i].Dump();
                            builtPlayers[i].Dump();
                        }
                    }

                    //currentGame.Teams[0].Dump();
                    //builtGame.Teams[0].Dump();
                }

                currentGame.Teams[1].Equals(builtGame.Teams[1]).Dump("Home Teams");
                if (!equal)
                {
                    currentGame.Teams[1].Dump();
                    builtGame.Teams[1].Dump();
                }



                //Console.WriteLine($"Game {key} is not equal");
                //Console.WriteLine($"Current Game: ");
                //Console.WriteLine($"Built Game: {builtGame}");
            }
        }
    }
}


