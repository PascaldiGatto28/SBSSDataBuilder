using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Application.Support;
using SBSSData.Softball.Logging;

namespace SBSSData.Application.DataStore
{
    /// <summary>
    /// The publishing the data store and web files to the sbssdata.info web site.
    /// </summary>
    /// <remarks>
    /// An instance of the class is created by the console application entry point <see cref="Program.Main"/> and then the
    /// <see cref="WebPublisher.Run(string?, bool?)"/> method is called which uses the WinSCP FTP publisher to move the Data Viewer
    /// files to the web site.
    /// </remarks>
    public sealed class WebPublisher
    {
        private static readonly AppSettings Settings = AppContext.Instance.Settings;
        private static readonly Log log = AppContext.Instance.Log;


#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved
        /// <summary>
        /// Executes the code that builds the HTML files and using FTP copies all changes files in the local web site to the
        /// web server.
        /// to the 
        /// </summary>
        /// <remarks>
        /// The <see cref="Construction.BuildHtml{T}(bool)"/> method invokes the methods for each type to construct the HTML and
        /// copies the constructed HTML to subfolder of the Data Store folder ("HtmlData" is the current folder) specified by
        /// the <see paramref="htmlLocation"/> parameter. When that's completed the 
        /// <see cref="Utilities.PublishSBSSData(string, bool)"/> method is invoked which copies all the changed files in Html Data
        /// folder to the web server using FTP.
        /// </remarks>
        /// <param name="htmlLocation">The subfolder of the Data Store in which local files for the web are stored.</param>
        /// <param name="isTest">If <c>true</c>>, the local web site is copied to the test folder of the web server; otherwise it
        /// copied to the production folder.
        /// </param>
        public static void Run(string? htmlLocation, bool? isTest)
#pragma warning restore CS1574 // XML comment has cref attribute that could not be resolved
        {
            string season = Settings.Season;
            Construction construction = new Construction(season);

            log.WriteLine("Beginning building HTML files that rely on the updated Data Store");

            construction.Callback = (o) => log.WriteLine($"{o.GetType().Name} updated and copied to {construction.OutputPath}{o.GetType().Name}.html");
            _ = construction.Build<DataStoreInfo>(true);
            _ = construction.Build<LogSessions>(true);
            _ = construction.Build<GamesTeamPlayersV3>(true);
            _ = construction.Build<GamesTeamPlayersHelpV3>(true);
            _ = construction.Build<PlayerSheets>(true);
            _ = construction.Build<PlayerSheetsGuide>(true);

            string htmlPath = htmlLocation ?? Settings.HtmlLocation;
            bool testing = isTest ?? Settings.IsTest;
            string webSiteType = testing ? "/quietcre/TestSync" : "/quietcre/Data";


            log.WriteLine($"Copying files from \"{htmlPath}\" to the \"{webSiteType}\" server folder");
            try
            {
                WinSCPSyncResults results = Utilities.PublishSBSSData(htmlPath, testing);
                log.WriteLine($"FTP to web server results:\r\n{results}");
            }
            catch (InvalidOperationException exception)
            {
                log.WriteLine(exception);
            }

            log.WriteLine("Construction of HTML files synchronization with the web server completed.");
        }
    }
}
