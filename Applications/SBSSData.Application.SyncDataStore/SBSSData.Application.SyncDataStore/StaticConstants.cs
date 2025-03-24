using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSSData.Application.SyncDataStore
{
    /// <summary>
    /// Encapsulates the folder and file paths to recover data and save updated data
    /// </summary>
    public static class StaticConstants
    {
        public static string Season = "2025Winter";
        public static string DSPath = $@"J:\SBSSDataStore\{Season}LeaguesData.json";
        public static string CompareDataFolder = @"J:\SBSSDataStoreComparisonTest\";
        public static string ConstructedDSPath = $@"{CompareDataFolder}{Season}BuiltLeaguesData.json";
        public static string CurrentDSPath = $@"{CompareDataFolder}{Season}CurrentLeaguesData.json";
        public static string CurrentGamesPath = $@"{CompareDataFolder}{Season}CurrentGames.json";
        public static string BuiltGamesPath = $@"{CompareDataFolder}{Season}BuiltGames.json";
        public static string UnEqualCurrentGamesPath = $@"{CompareDataFolder}{Season}UnEqualCurrentGames.json";
        public static string UnEqualBuiltGamesPath = $@"{CompareDataFolder}{Season}UnEqualBuiltGames.json";
        public static List<string> ExcludedGames = [ "31221", "31380", "31381" ];
    }
}
