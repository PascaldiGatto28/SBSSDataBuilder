using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SBSSData.Softball;

namespace SBSSData.Application.SyncDataStore
{
    public class CompareResults
    {
        public CompareResults()
        {
        }

        //public SortedList<string, Game> CurrentGames { get; set; } = [];
        //public SortedList<string, Game> BuiltGames { get; set; } = [];

        public List<string> UnequalGames { get; set; } = [];
        public List<string> CurrentKeysNotFound { get; set; } = [];
        public List<string> BuiltKeysNotFound { get; set; } = [];

        public int UnequalCount => GetCount(UnequalGames);
        public int CurrentKeysNotFoundCount => GetCount(CurrentKeysNotFound);
        public int BuiltKeysNotFoundCount => GetCount(BuiltKeysNotFound);

        private int GetCount(List<string> list)
        {
            int count = list.Count;
            if ((count == 1) && !int.TryParse(list[0], out int value))
            {
                count = 0;
            }

            return count;
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Unequal games: {UnequalCount}");
            sb.AppendLine($"Current games not in built: {CurrentKeysNotFoundCount}");
            sb.AppendLine($"Built games not in current: {BuiltKeysNotFoundCount}");
            return sb.ToString();
        }
    }

    public class DisplayCompareResults : CompareResults
    {
        public DisplayCompareResults()
        {
        }

        //public DisplayCompareResults(CompareResults results)
        //{
        //    Results = results;
        //}   

        //public CompareResults Results
        //{
        //    get; set;
        //}

        public new List<string> UnequalGames => base.UnequalGames;
        public new List<string> CurrentKeysNotFound => base.CurrentKeysNotFound;
        public new List<string> BuiltKeysNotFound => base.BuiltKeysNotFound;
        public new int UnequalCount => UnequalGames.Count;
        public new int CurrentKeysNotFoundCount => CurrentKeysNotFound.Count;
        public new int BuiltKeysNotFoundCount => BuiltKeysNotFound.Count;
    }
}
  
