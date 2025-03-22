using System.Diagnostics;
using Dumpify;
using SBSSData.Softball;

namespace SBSSData.Application.SyncDataStore
{
    public class Compare
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int dsSize = Support.ConstructCurrent(StaticConstants.ConstructedDSPath, false);
            sw.Stop();
            Console.WriteLine($"Data store size: {dsSize:#,##0} bytes, and took {sw.ElapsedMilliseconds/1000:#,###.#} seconds");

            File.Copy(StaticConstants.DSPath, StaticConstants.CurrentDSPath, true);

            SortedList<string, Game> currentGames = Support.GetSortedGames(StaticConstants.CurrentDSPath);
            Console.WriteLine($"Current data store has {currentGames.Count} games");
           
            SortedList<string, Game> builtGames = Support.GetSortedGames(StaticConstants.ConstructedDSPath);
            Console.WriteLine($"Built data store has {builtGames.Count} games");

            CompareResults results = Support.CompareGames(currentGames, builtGames);
            results.Dump();
            //Tuple<List<string>, List<string>> compareResults = Support.CompareGames(currentGames, builtGames);
            //int unequalCount = compareResults.Item1.Count; 
            //int keysNotFoundCount = compareResults.Item2.Count;

            if (results.UnequalCount == 0)
            {
                Console.WriteLine("All games are equal");
            }
        }
    }
}