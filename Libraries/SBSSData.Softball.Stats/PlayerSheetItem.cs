﻿using SBSSData.Softball.Common;


namespace SBSSData.Softball.Stats
{
    public class PlayerSheetItem
    {
        public PlayerSheetItem() 
        {
            PlayerName = string.Empty;
            LeagueName = string.Empty;
            NumGames = 0;
            NumTeams = 0;
            LeagueNumGames = 0;
            LeagueNumTeams = 0;
            PlayerTotals = Player.Empty;
            LeagueTotals = Player.Empty;
            PlayerPercentiles = [];
            LeagueStatistics = [];
        }

        public string PlayerName { get; set; }

        public string LeagueName { get; set; }

        public int NumGames { get; set; } 

        public int NumTeams { get; set; }

        public Player PlayerTotals{ get; set; }

        public Player LeagueTotals { get; set; }

        public int LeagueNumGames { get; set; }

        public int LeagueNumTeams { get; set; }

        public string Description => $"""
                                      In the {LeagueName} League {PlayerName.BuildDisplayName()} played on {NumTeams.NumDesc("team")} and in {NumGames.NumDesc("game")}
                                      """;

        public string LeagueDescription => $"""
                                            For the {LeagueName} League, there are {LeagueNumTeams.NumDesc("team")} and {LeagueNumGames.NumDesc("played game")}
                                            """;

        public string DisplayName => PlayerName.BuildDisplayName();

        public PlayerStats PlayerTotalsStats => new(PlayerTotals, NumGames);

        public PlayerStats LeagueTotalsStats => new(LeagueTotals, LeagueNumGames);

        public List<PlayerSheetPercentile> PlayerPercentiles 
        { 
            get; 
            set; 
        }

        public List<LeagueStatistics> LeagueStatistics 
        {
            get;
            set; 
        }
    }
}
