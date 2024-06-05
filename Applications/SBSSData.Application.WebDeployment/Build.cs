using SBSSData.Application.LinqPadQuerySupport;
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
        private static readonly bool PublishToTest = settings.Test;

        public Build()
        { 
        }

        public void Run()
        {
            bool buildHtml = true;
            bool publish = true;
            bool publishToTest = true; //PublishToTest; // (args == null) || (args.Length == 0) || (args[0] == "Test");


            if (buildHtml)
            {
                string[] seasons = ["2024 Spring", "2024 Winter", "2023 Fall", "2023 Summer"];
                Construction construction = new()
                {
                    SeasonText = seasons[0],
                    DsFolder = DataStoreFolder,
                    HtmlFolder = HtmlData,
                    Callback = (t) => log.WriteLine(t.ToString() ?? "Bad logging comment!")
                };

                _ = construction.Build<DataStoreInfo>(true);
                _ = construction.Build<LogSessions>(true);

                foreach (string season in seasons)
                {
                    log.WriteLine($"Beginning construction of HTML pages for {season}");
                    construction.SeasonText = season;
                    _ = construction.Build<GamesTeamPlayersV3>(true);
                    _ = construction.Build<GamesTeamPlayersHelpV3>(true);
                    _ = construction.Build<PlayerSheets>(true);
                    _ = construction.Build<PlayerSheetsGuide>(true);
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
