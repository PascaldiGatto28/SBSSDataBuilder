using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record DSInformationDisplay(string Season,
                                       string LastUpdated,
                                       int Leagues,
                                       string DataStoreFileSize,
                                       int ScheduledGames,
                                       int CompletedGames,
                                       int PlayedGames,
                                       int CanceledGames,
                                       int ForfeitedGames,
                                       int Teams,
                                       int Players)
    {
        public DSInformationDisplay(string season, DataStoreContainer dsContainer) : 
                                                          this(Utilities.SwapSeasonText(season),
                                                          dsContainer.DataStore.BuildDate.ToString("dddd MMMM d, yyyy"),
                                                          dsContainer.DataStore.LeagueSchedules.Count(),
                                                          //dsContainer.DataStorePath,
                                                          dsContainer.DataStoreSize.FormatInt(extend: true),
                                                          dsContainer.NumberOfGames,
                                                          dsContainer.GamesCompleted,
                                                          dsContainer.GamesPlayed,
                                                          dsContainer.GamesCanceled,
                                                          dsContainer.GamesForfeited,
                                                          dsContainer.NumberOfTeams,
                                                          dsContainer.NumberOfPlayers)
        { }


    }
}
