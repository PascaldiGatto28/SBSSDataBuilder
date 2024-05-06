
using Dumpify;

using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Application.Support;

namespace SBSSData.Application.WebDeployment
{
    internal class Build
    {
        private static void Main(string[] args)
        {
            bool buildHtml = false;
            bool publish = true;
            bool publishToTest = true;


            if (buildHtml)
            {
                string[] seasons = ["2024 Spring", "2024 Winter", "2023 Fall", "2023 Summer"];
                foreach (string season in seasons)
                {
                    Construction construction = new(season);
                    _ = construction.Build<DataStoreInfo>(true);
                    _ = construction.Build<LogSessions>(true);
                    _ = construction.Build<GamesTeamPlayersV3>(true);
                    _ = construction.Build<GamesTeamPlayersHelpV3>(true);
                    _ = construction.Build<PlayerSheets>(true);
                    _ = construction.Build<PlayerSheetsGuide>(true);
                }
            }


            //Construction constructionSync = new();
            //constructionSync.WinSCPSync(true);  // true is testing (default) false is production

            if (publish)
            {

                WinSCPSyncResults results = Utilities.PublishSBSSData(isTest:publishToTest);
                results.Dump();
            }
        }
    }
}
