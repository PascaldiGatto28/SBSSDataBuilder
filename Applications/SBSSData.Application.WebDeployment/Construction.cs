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

        private static Action<object> Callback = (o) => o.Dump();
        private void DisplayInBrowser(string filePath) => Process.Start(edgeFolder, $"\"{filePath}\"");

        public Func<IHtmlCreator, Action<object>?, string> BuildHtml => (i, a) => i.BuildHtmlPage(SeasonText, DsFolder, a ?? Callback);

        public string Build<T>(bool useCallback) where T : IHtmlCreator, new()
        {
            string html = string.Empty;
            T htmlCreator = (T)Activator.CreateInstance(typeof(T));
            WriteOutput(typeof(T).Name, htmlCreator.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null));
            return html;
        }


        public void WinSCPSync(bool isTest = true)
        {
            // Set up session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = "ftp.walkingtree.com",
                UserName = "quietcre",
                Password = "85232WindingWay",
            };

            string target = isTest ? "/quietcre/Data/Test" : "/quietcre/Data";
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
                            target, false);

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

                Console.WriteLine($"WinSCP synchronization to {target} completed {isError}");
            }
        }
    }
}