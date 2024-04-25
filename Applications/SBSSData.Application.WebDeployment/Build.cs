using SBSSData.Application.LinqPadQuerySupport;

namespace SBSSData.Application.WebDeployment
{
    internal class Build
    {
        private static void Main(string[] args)
        {
            string[] seasons = ["2024 Winter"];
            foreach (string season in seasons)
            {
                Construction construction = new(season);
                construction.Build<LogSessions>(true);
                construction.Build<DataStoreInfo>(true);
                //construction.Build<GamesTeamPlayersV3>(true);
                //construction.Build<GamesTeamPlayersHelpV3>(true);
                //construction.Build<PlayerSheets>(true);
                //construction.Build<PlayerSheetsGuide>(true);
            }


            //Construction constructionSync = new();
            //constructionSync.WinSCPSync();
        }
    }
}
