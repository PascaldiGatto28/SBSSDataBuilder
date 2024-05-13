// Ignore Spelling: npp

using System.Diagnostics;

using Dumpify;

using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Softball.Common;

using AppContext = SBSSData.Application.Infrastructure.AppContext;


namespace SBSSData.Application.WebDeployment
{
    public class Construction
    {
        public static readonly string edgeFolder = @"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe";
        public static readonly string nppFolder = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
        public static readonly string dsFolder = AppContext.Instance.Settings.DataStoreFolder;
        public static readonly string htmlFolder = AppContext.Instance.Settings.HtmlFolder;

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

        public Construction(string seasonText, 
                            string dsFolder, 
                            string htmlFolder,  
                            Action<object>? callback) 
        {
            SeasonText = seasonText;
            DsFolder = dsFolder;
            HtmlFolder = htmlFolder;
            Callback = callback;
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

        public string OutputPath => $"{dsFolder}{HtmlFolder}{SeasonText.RemoveWhiteSpace()}\\";


        public void WriteOutput(string fileName, string html, bool displayHtml = false, bool displayConsole = false)
        {
            string htmlFilePath = $"{OutputPath}{fileName}.html";
            if ((fileName == "LogSessions") || (fileName == "DataStoreInfo"))
            {
               htmlFilePath = $"{dsFolder}{HtmlFolder}{fileName}.html";
            }

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

        public Action<object>? Callback
        {
            get;
            set;
        } = (o) => o.Dump();

        private void DisplayInBrowser(string filePath) => Process.Start(edgeFolder, $"\"{filePath}\"");

        public Func<IHtmlCreator, Action<object>?, string> BuildHtml => (i, a) => i.BuildHtmlPage(SeasonText, DsFolder, a ?? Callback);

        public string Build<T>(bool useCallback) where T : IHtmlCreator, new()
        {
            string html = string.Empty;
            T? htmlCreator = (T?)Activator.CreateInstance(typeof(T));

            if (htmlCreator != null)
            {
                WriteOutput(typeof(T).Name, htmlCreator.BuildHtmlPage(SeasonText, DsFolder, useCallback ? Callback : null));
            }

            return html;
        }


        
    }
}