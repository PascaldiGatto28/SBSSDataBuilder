using System.Diagnostics;

using Dumpify;

using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Softball.Common;

using WinSCP;

namespace SBSSData.Application.WebDeployment
{
    public class Construction
    {
        public static readonly string edgeFolder = @"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe";
        public static readonly string nppFolder = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
        public static readonly string dsFolder = $@"J:\SBSSDataStore\";
        public static readonly string htmlFolder = @"HtmlData\";

        public Construction()
        {
            DsFolder = dsFolder;
            SeasonText = string.Empty;
            HtmlFolder = string.Empty;
        }

        public Construction(string seasonText): this()
        {
            SeasonText = seasonText;

            // For example, HtmlData\2024Winter\
            HtmlFolder = $"{htmlFolder}{seasonText.RemoveWhiteSpace()}\\";
        }


        public string SeasonText
        {
            get;
            set;
        }

        public string DsFolder
        {
            get;
            set;
        }

        public string HtmlFolder
        {
            get;
            set;
        }

        public string OutputPath => $"{DsFolder}{HtmlFolder}";


        public void WriteOutput(string fileName, string html, bool displayHtml = false, bool displayConsole = true)
        {
            string htmlFilePath = $"{OutputPath}{fileName}.html";
            File.WriteAllText(htmlFilePath, html);

            if (displayHtml)
            {
                DisplayInBrowser(htmlFilePath);
            }

            if (displayConsole)
            {
                Console.WriteLine($"{fileName} completed");
            }

        }


        private Func<IHtmlCreator, bool, string> createHtml => (i, c) => i.BuildHtmlPage(SeasonText, DsFolder, c ? Callback : null);

        public string Build<T>(bool useCallback) where T : IHtmlCreator, new()
        {
            string html = string.Empty;
            T htmlCreator = (T)Activator.CreateInstance(typeof(T));
            WriteOutput(typeof(T).Name, htmlCreator.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null));
            return html;
        }

        //public void LogSessions(bool displayHtml = false, bool useCallback = false)
        //{
        //    //LogSessions ls = new();
        //    //string html = ls.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null);
        //    WriteOutput("LogSessions", logSessionsHtml, displayHtml:true);
        //}

        public void GamesTeamPlayersV3(bool displayHtml = false, bool useCallback = false)
        {
            GamesTeamPlayersV3 gtp = new GamesTeamPlayersV3();
            string html = gtp.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null);

            string folderName = @$"{DsFolder}{htmlFolder}";
            string fileName = "GamesTeamPlayersV3.html";
            string htmlFilePath = $"{OutputPath}{fileName}";

            File.WriteAllText(htmlFilePath, html);

            if (displayHtml)
            {
                DisplayInBrowser(htmlFilePath);
            }

            Console.WriteLine("GamesTeamPlayersV3 completed");
        }

        public void GamesTeamPlayersHelpV3(bool displayHtml = false, bool useCallback = false)
        {
            GamesTeamPlayersHelpV3 gtph = new GamesTeamPlayersHelpV3();
            string html = gtph.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null);

            string folderName = @$"{DsFolder}{htmlFolder}";
            string fileName = "GamesTeamPlayersHelpV3.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            if (displayHtml)
            {
                DisplayInBrowser(htmlFilePath);
            
            }

            Console.WriteLine("GamesTeamPlayersHelpV3 completed");
        }

    public void PlayerSheets(bool displayHtml = false, bool useCallback = false)
    {
        PlayerSheets playerSheets = new PlayerSheets();
        string html = playerSheets.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null);

        string folderName = @$"{DsFolder}{htmlFolder}";
        string fileName = "PlayerSheets.html";
        string htmlFilePath = $"{folderName}{fileName}";

        File.WriteAllText(htmlFilePath, html);
        if (displayHtml)
        {
            DisplayInBrowser(htmlFilePath);
        }

        Console.WriteLine("PlayerSheets completed");
    }

    public void PlayerSheetsGuide(bool displayHtml = false, bool useCallback = false)
    {
        PlayerSheetsGuide playerSheetsGuide = new PlayerSheetsGuide();
        string html = playerSheetsGuide.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null);

        string folderName = @$"{DsFolder}{htmlFolder}";
        string fileName = "PlayerSheetsGuide.html";
        string htmlFilePath = $"{folderName}{fileName}";

        File.WriteAllText(htmlFilePath, html);
        if (displayHtml)
        {
            DisplayInBrowser(htmlFilePath);
        }

        Console.WriteLine("PlayerSheetsGuide completed");
    }

        public  void WinSCPSync()
        {
            // Set up session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = "ftp.walkingtree.com",
                UserName = "quietcre",
                Password = "85232WindingWay",
            };

            using (Session session = new Session())
            {
                //session.FileTransferred = (s,e) => e.Dump();
                // Connect
                session.Open(sessionOptions);

                var comparison = session.CompareDirectories(SynchronizationMode.Local, @"J:\SBSSDataStore\HtmlData", "/quietcre/Data", true);

                string isError = string.Empty;
                try
                {
                    // Synchronize files
                    SynchronizationResult synchronizationResult;
                    synchronizationResult =
                        session.SynchronizeDirectories(
                            SynchronizationMode.Remote,
                            @"J:\SBSSDataStore\HtmlData",
                            "/quietcre/Data", false);

                    synchronizationResult.Check();
                    Console.WriteLine($"Failures {synchronizationResult.Failures.Count}");
                    Console.WriteLine($"Uploads {synchronizationResult.Uploads.Count}");
                    Console.WriteLine($"Downloads {synchronizationResult.Downloads.Count}");
                }
                catch (Exception exception)
                {
                    isError = $" with error {exception.Message}";
                    Console.WriteLine($"Error: {exception}");
                }

                Console.WriteLine($"WinSCP synchronization completed{isError}");
            }
        }


        private static Action<object> Callback = (o) => o.Dump();
        private void DisplayInBrowser(string filePath) => Process.Start(edgeFolder, $"\"{filePath}\""); 
    }
}