﻿using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record DataStoreInformation(string DateOfLastUpdate,
                                       string NumberOfLeagues,
                                       //string DataStoreFilePath,
                                       string DataStoreSize,
                                       string NumberOfScheduledGames,
                                       string GamesCompleted,
                                       string GamesPlayed,
                                       string GamesCanceled,
                                       string GamesForfeited,
                                       string NumberOfTeams,
                                       string NumberOfPlayers)
    {
        public DataStoreInformation(DataStoreContainer dsContainer) : this(dsContainer.DataStore.BuildDate.ToString("dddd MMMM d, yyyy a\\t h:mm:ss tt"),
                                                                           dsContainer.DataStore.LeagueSchedules.Count().ToString(),
                                                                           //dsContainer.DataStorePath,
                                                                           dsContainer.DataStoreSize.FormatInt(extend: true),
                                                                           dsContainer.NumberOfGames.ToString(),
                                                                           dsContainer.GamesCompleted.ToString(),
                                                                           dsContainer.GamesPlayed.ToString(),
                                                                           dsContainer.GamesCanceled.ToString(),
                                                                           dsContainer.GamesForfeited.ToString(),
                                                                           dsContainer.NumberOfTeams.ToString(),
                                                                           dsContainer.NumberOfPlayers.ToString())
        { }


    }
}
