using Newtonsoft.Json;

using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Logging;

using AppContext = SBSSData.Application.Infrastructure.AppContext;

namespace SBSSData.Application.DataStore
{
    /// <summary>
    /// Creates static HTML pages that depend on the current data store, and publishes any changes to the SBSSData.info
    /// web site.
    /// </summary>
    /// <remarks>
    /// An instance of this class is invoked by the main <see cref="Program"/> if the <see cref="DataStoreManager"/> creates
    /// or updates the current data store.
    /// </remarks>
    /// <remarks>
    /// The default constructor that creates the instance using specified or default property values.
    /// </remarks>
    public class HtmlDeployment(Log activeLog, string dataStoreFolder = "", string htmlDataFolder = "", string season = "")
    {
        public Log ActiveLog
        {
            get;
            set;
        } = activeLog;

        public string DataStoreFolder
        {
            get;
            init;
        } = string.IsNullOrEmpty(dataStoreFolder) ? AppContext.Instance.Settings.DataStoreFolder : dataStoreFolder;

        public string HtmlDataFolder
        {
            get;
            init;
        } = string.IsNullOrEmpty(htmlDataFolder) ? AppContext.Instance.Settings.HtmlLocation : htmlDataFolder;

        public string Season
        {
            get;
            init;
        } = string.IsNullOrEmpty(season) ? AppContext.Instance.Settings.Season : season;

        private string OutputPath => $"{HtmlDataFolder}{Season.RemoveWhiteSpace()}\\";

        public void CreateHtml(bool modified) 
        {
            if (modified)
            {

                Directory.CreateDirectory(OutputPath);

                ActiveLog.WriteLine($"Beginning construction of HTML pages for {Season}");

                Build<DataStoreInfo>(true);
                Build<GamesTeamPlayersV3>(true);
                Build<GamesTeamPlayersHelpV3>(true);
                Build<PlayerSheets>(true);
                Build<PlayerSheetsGuide>(true);
                Build<SortablePlayerStats>(true);

                ActiveLog.WriteLine("Six HTML pages have been constructed.");
            }
            else
            {
                ActiveLog.WriteLine("No HTML pages constructed, because the data store has not changed.");
            }
        }

        // This method must be called after any logging, because it to create the LogSessions HTML file it must
        // create the JSON file from the log file, and that requires the log file to be closed.
        public void FinishDeployment(bool deployToWeb, bool deploymentTest = false)
        {
            string where = deploymentTest ? "Test Sync" : "Production (Data)";
            ActiveLog.WriteLine($"Deploying changed HTML pages to the {where} folder on sbssdata.info");

            if (deployToWeb)
            {
                WinSCPSyncResults results = Utilities.PublishSBSSData($"{HtmlDataFolder}", deploymentTest);
                ActiveLog.WriteLine($"{results}");
            }

            ActiveLog.WriteLine("Completed.");
            ActiveLog.Close();

            BuildJsonLogSessions(ActiveLog.LogFilePath);
            Build<LogSessions>(true);
            string copyResults = Utilities.PublishSingleFile($"{HtmlDataFolder}LogSessions.html", deploymentTest);

            // The log ain't no good no more, so we just have to wright out the results to the console.
            Console.WriteLine($"Publishing LogSessions.html: {copyResults}"); 
        }

        public Func<IHtmlCreator, Action<object>?, string> BuildHtml => (i, a) => i.BuildHtmlPage(Season, DataStoreFolder, a ?? Callback);

        public string Build<T>(bool useCallback) where T : IHtmlCreator, new()
        {
            string html = string.Empty;
            T? htmlCreator = (T?)Activator.CreateInstance(typeof(T));

            if (htmlCreator != null)
            {
                WriteOutput<T>(htmlCreator.BuildHtmlPage(Season, DataStoreFolder, useCallback ? Callback : null));
                
            }

            return html;
        }

        public Action<object>? Callback
        {
            get;
            set;
        } = (o) => Console.WriteLine($"{o}");

        public void WriteOutput<T>(string html)
        {
            string fileName = typeof(T).Name;
            string htmlFilePath = $"{OutputPath}{fileName}.html";

            if ((fileName == "LogSessions") || (fileName == "DataStoreInfo"))
            {
                htmlFilePath = $"{HtmlDataFolder}{fileName}.html";
            }

            File.WriteAllText(htmlFilePath, html);
            if (!ActiveLog.IsClosed)
            {
                ActiveLog.WriteLine($"{typeof(T).Name} created and saved.");
            }

        }
        public static string BuildJsonLogSessions(string logFilePath)
        {
            string json = string.Empty;
            if (!string.IsNullOrEmpty(logFilePath))
            {
                //string logFilePath = context.Settings.LogFilePath;
                IEnumerable<LogSession> sessions = Log.ReadLog(logFilePath);
                string logSessionsFilePath = logFilePath.Replace(".log", ".json", StringComparison.Ordinal);
                IEnumerable<LogSession>? loggedSessions = File.Exists(logSessionsFilePath) ?
                                                         JsonConvert.DeserializeObject<IEnumerable<LogSession>>(File.ReadAllText(logSessionsFilePath)) :
                                                         [];

                if ((loggedSessions != null) && loggedSessions.Any())
                {
                    sessions.ToList().AddRange(loggedSessions);
                }

                json = JsonConvert.SerializeObject(sessions).FormatJsonString();
                File.WriteAllText(logSessionsFilePath, json);
            }

            return json;
        }
    }
}
