// Ignore Spelling: npp

using System.Diagnostics;

using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Softball.Common;

namespace SBSSData.Application.DataStore
{
    public class Construction
    {
        public static readonly string edgeFolder = @"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe";
        public static readonly string nppFolder = @"C:\Program Files (x86)\Notepad++\notepad++.exe";
        public static readonly string dsTopFolder = $@"J:\SBSSDataStore\";
        public static readonly string htmlTopFolder = @"HtmlData\";

        public Construction()
        {
            DsFolder = dsTopFolder;
            SeasonText = string.Empty;
            HtmlFolder = htmlTopFolder;
        }

        public Construction(string seasonText): this()
        {
            SeasonText = seasonText;

            // For example, HtmlData\2024Winter\
            HtmlFolder = $"{htmlTopFolder}{seasonText.RemoveWhiteSpace()}\\";
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

        public Action<object> Callback
        {
            get;
            set;
        }

        private static void DisplayInBrowser(string filePath) => Process.Start(edgeFolder, $"\"{filePath}\"");

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