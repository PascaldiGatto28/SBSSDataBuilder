using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Application.Support;
using SBSSData.Softball.Logging;

using AppContext = SBSSData.Application.Infrastructure.AppContext;
using AppSettings = SBSSData.Application.Infrastructure.AppSettings;


namespace SBSSData.Application.WebDeployment
{
    public sealed class TestBuild
    {
        private static readonly AppSettings settings = AppSettings.Instance($@"Configuration\Settings.json");
        private static readonly string DataStoreFolder = settings.DataStoreFolder;
        private static readonly Log log = AppContext.Instance.Log;
        private static readonly string HtmlData = settings.HtmlFolder;
        //private static readonly bool PublishToTest = settings.Test;

        public TestBuild()
        { 
        }

        public static void Run()
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
                _ = construction.Build<SortablePlayerStats>(true);
                _ = construction.Build<GamesTeamPlayersV3>(true);
                _ = construction.Build<GamesTeamPlayersHelpV3>(true);
                _ = construction.Build<PlayerSheets>(true);
                _ = construction.Build<PlayerSheetsGuide>(true);
            }
            
        }
    }
}
