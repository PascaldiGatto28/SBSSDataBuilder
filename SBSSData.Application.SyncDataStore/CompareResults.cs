using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSSData.Application.SyncDataStore
{
    public class CompareResults
    {
        public CompareResults()
        {
        }

        public List<string> UnequalGames { get; set; } = [];
        public List<string> CurrentKeysNotFound { get; set; } = [];
        public List<string> BuiltKeysNotFound { get; set; } = [];

        public int UnequalCount => UnequalGames.Count;
        public int CurrentKeysNotFoundCount => CurrentKeysNotFound.Count;
        public int BuiltKeysNotFoundCount => BuiltKeysNotFound.Count;
    }
}
