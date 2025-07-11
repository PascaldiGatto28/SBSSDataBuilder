using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Dumpify;

namespace SBSSData.Application.SyncDataStore
{
    public class Program()
    {
        public static void Main()
        {
            DumpConfig.Default.TypeNamingConfig.ShowTypeNames = false;
            DumpConfig.Default.TableConfig.ShowTableHeaders = false;
            DumpConfig.Default.TableConfig.ShowRowSeparators = true;

            Compare.CompareGamesEX();
        }
    }
}
