using Dumpify;

using SBSSData.Application.Support;
using SBSSData.Softball.Logging;

using AppContext = SBSSData.Application.Infrastructure.AppContext;
using AppSettings = SBSSData.Application.Infrastructure.AppSettings;


namespace SBSSData.Application.WebDeployment
{
    public sealed class Build
    {
        private static readonly AppSettings settings = AppSettings.Instance($@"Configuration\Settings.json");
        private static readonly string DataStoreFolder = settings.DataStoreFolder;
        private static readonly Log log = AppContext.Instance.Log;
        private static readonly string HtmlData = settings.HtmlFolder;
        //private static readonly bool PublishToTest = settings.Test;

        public Build()
        { 
        }

        public static void Run(string[] args)
        {
            bool buildHtml = true;
            bool publish = false;
            bool publishToTest = true; // (args == null) || (args.Length == 0) || (args[0] == "Test");

            args.Dump("Data Store Changed?");


            if (buildHtml)
            {
                Construction construction = new()
                {
                    SeasonText = StaticConstants.Seasons[0],
                    DsFolder = DataStoreFolder,
                    HtmlFolder = HtmlData,
                    Callback = (t) => log.WriteLine(t.ToString() ?? "Bad logging comment!")
                };

                //_ = construction.Build<DataStoreInfo>(true);
                //_ = construction.Build<LogSessions>(true);

                //foreach (string season in StaticConstants.Seasons)
                string season = "2024 Summer";
                {
                    log.WriteLine($"Beginning construction of HTML pages for {season}");
                    construction.SeasonText = season;
                    //_ = construction.Build<GamesTeamPlayersV3>(true);
                    //_ = construction.Build<GamesTeamPlayersHelpV3>(true);
                    //_ = construction.Build<PlayerSheets>(true);
                    //_ = construction.Build<PlayerSheetsGuide>(true);
                    //_ = construction.Build<SortablePlayerStats>(true);
                    //_ = construction.Build<SortablePlayerStatsGuide>(true);
                }
            }

            if (publish)
            {
                string where = publishToTest ? "testing" : "production";
                log.WriteLine($"Publishing to the {where} folder on sbssdata.info");
                WinSCPSyncResults results = Utilities.PublishSBSSData(isTest: publishToTest);
                log.WriteLine(results.ToString());
            }
            else
            {
                log.WriteLine("HTML pages have been constructed but not published.");
            }
        }
    }
}
