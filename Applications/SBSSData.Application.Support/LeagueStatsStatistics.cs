using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record LeagueStatsStatistics(LeagueName League, IEnumerable<PlayerStatsDisplay> PlayersStatsDisplay, IEnumerable<StatisticsDisplay> PropertiesStatistics)
    {
    }
}
