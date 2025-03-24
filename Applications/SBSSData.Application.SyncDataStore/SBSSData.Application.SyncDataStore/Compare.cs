using System.Diagnostics;
using System.Text;

using Dumpify;
using SBSSData.Softball;
using SBSSData.Softball.Common;

namespace SBSSData.Application.SyncDataStore
{
    public class Compare
    {
        public static void CompareGamesEX()
        {
            
            Stopwatch sw = new Stopwatch();
            sw.Start();
            int dsSize = Support.ConstructCurrent(StaticConstants.ConstructedDSPath, false);
            sw.Stop();
            int seconds = (int)sw.ElapsedMilliseconds / 1000;
            string elapsedTime = seconds > 0 ? $", and took {seconds:#,###} seconds" : ". The data store was present and not rebuilt.";
            StringBuilder summary = new();
            summary.AppendLine($"Data store size: {dsSize:#,##0} bytes{elapsedTime}");
            
            //Console.WriteLine($"Data store size: {dsSize:#,##0} bytes, and took {sw.ElapsedMilliseconds/1000:#,###.#} seconds");

            File.Copy(StaticConstants.DSPath, StaticConstants.CurrentDSPath, true);
            FileInfo fileInfo = new FileInfo(StaticConstants.DSPath);
            DateTime currentLastWrite = fileInfo.LastWriteTime;

            fileInfo = new FileInfo(StaticConstants.ConstructedDSPath);
            DateTime builtLastWrite = fileInfo.LastWriteTime;

            //List<string> excludedGames = StaticConstants.ExcludedGames;
            SortedList<string, Game> currentGames = Support.GetSortedGames(StaticConstants.CurrentDSPath);
            currentGames.Serialize(StaticConstants.CurrentGamesPath);
            summary.AppendLine($"Current data store has {currentGames.Count} games");
           
            SortedList<string, Game> builtGames = Support.GetSortedGames(StaticConstants.ConstructedDSPath);
            builtGames.Serialize(StaticConstants.BuiltGamesPath);
            summary.Append($"Built data store has {builtGames.Count} games");
            summary.ToString().Dump();

            CompareResults results = Support.CompareGames(currentGames, builtGames);
            
            string title = $"Comparing Results\r\nCurrent DS Date: {currentLastWrite}\r\nConstructed DS Date: {builtLastWrite}";
            results.Dump(title, typeNames: new TypeNamingConfig { ShowTypeNames = false }, tableConfig: new TableConfig { ShowTableHeaders = false, ShowRowSeparators = true });

            if (results.UnequalCount == 0)
            {
                string countText = "All games are equal";
                Console.WriteLine(countText);
                countText.Dump("Game Comparison Results");
            }

            string summaryText = Dumpify.DumpExtensions.DumpText(summary, "Summary Information");
            //html.Dump();
            string resultsText = results.DumpText(title, typeNames: new TypeNamingConfig { ShowTypeNames = false }, tableConfig: new TableConfig { ShowTableHeaders = false, ShowRowSeparators = true });
            File.WriteAllText($"{StaticConstants.CompareDataFolder}CompareResults-{DateTime.Now:MM-dd-yyyy}.txt", $"{summaryText}\r\n{resultsText}");

            string html = LINQPad.Util.ToHtmlString(summary, results);
            File.WriteAllText($"{StaticConstants.CompareDataFolder}CompareResults-{DateTime.Now:MM-dd-yyyy}.html", html);


            SortedList<string, Game> unEqualCurrentGames = currentGames.Where(s => results.UnequalGames.Contains(s.Key)).ToSortedList();
            unEqualCurrentGames.Serialize(StaticConstants.UnEqualCurrentGamesPath);

            SortedList<string, Game> unEqualBuiltGames = builtGames.Where(s => results.UnequalGames.Contains(s.Key)).ToSortedList();
            unEqualBuiltGames.Serialize(StaticConstants.UnEqualBuiltGamesPath);

            //Support.ViewUnEqualGames(results, unEqualCurrentGames, unEqualBuiltGames);

            Console.Write("Press any key to exit...");
            Console.ReadKey(false);
        }
    }
}