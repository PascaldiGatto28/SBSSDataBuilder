// Ignore Spelling: Linq

using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Logging;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class LogSessions : IHtmlCreator
    {
        public LogSessions()
        {
            Values = [];
        }

        public List<object> Values
        {
            get;
            set;
        }
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "Logging Information"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["Logging Information", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        private static string headerStyle = "background-color:#d62929;";

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            Assembly assembly = typeof(LogSessions).Assembly;
            string resName = assembly.FormatResourceName("LogSessions.html");
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();

            string changedHtml = string.Empty;


            string logPath = $@"{dataStoreFolder}DataStoreManager.json";
            IEnumerable<LogSession>? sessions = logPath.Deserialize<IEnumerable<LogSession>>();

            var displaySessions = sessions?.Select(s => new
            {
                //s.BuildDate,
                //SessionID = s.Session,
                LogEntries = s.LogEntries.Select(l => new
                {
                    Date = l.Date,
                    EventDescription = l.LogText
                })
            });

            using (HtmlGenerator generator = new HtmlGenerator())
            {
                string topOfTable = $"""
                                     <h2 style="font-size:1.1em; color:#3d5e8f; margin-top:20px; font-weight:500;">
                                       Table of all Data Store Updates Recorded by SBSS Data Viewer for the Current Season
                                     </h2>
                                     """;
                generator.WriteRawHtml(topOfTable);
                generator.WriteRootTable(displaySessions, LogSessionsCallback);
                if ((callback != null) && (displaySessions != null))
                {
                    callback($"{this.GetType().Name} HTML page created.");
                }

                string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                HtmlNode title = HtmlNode.CreateNode(htmlNode);
                changedHtml = generator.DumpHtml(pageTitle: title,
                                                 cssStyles: StaticConstants.LocalStyles,
                                                 javaScript: StaticConstants.LocalJavascript,
                                                 collapseTo: 2,
                                                 headElements: headElements.ToList());
            }

            return changedHtml;
        }

        public static Func<TableNode, string> LogSessionsCallback = (t) =>
        {
            HtmlNode tableNode = t.TableHtmlNode;
            string header = string.Empty;
            if (t.Depth() == 0)
            {
                int numSessions = t.ChildNodes.Count();
                header = $"{numSessions} Recorded Log Sessions";
                tableNode.SelectSingleNode("./thead/tr/td").Attributes.Add("style", headerStyle);
                tableNode.SelectSingleNode("./thead/tr[last()]").Attributes.Add("style", "display:none");
            }
            else
            {
                IEnumerable<string> textCells = tableNode.SelectNodes("./tbody//tr/td[2]").Select(n => n.InnerText);
                string? cell = textCells.SingleOrDefault(t => t.Contains("game") && t.Contains("updated"));
                int numUpdated = 0;
                if (!string.IsNullOrEmpty(cell))
                {
                    int index = cell.IndexOf(' ');
                    int.TryParse(cell.Substring(0, index), out numUpdated);
                }

                HtmlNodeCollection tableColumnHeaders = tableNode.SelectNodes("./thead/tr[2]//th");
                tableColumnHeaders[0].SetAttributeValue("title", "Date and time the event was recorded");
                tableColumnHeaders[1].SetAttributeValue("title", "The description of the log event");
                header = (numUpdated > 0) ? $"{numUpdated.NumDesc("Scheduled Game").Capitalize()} Updated"
                                          : "No Scheduled Games Were Updated &mdash; The Data Store is Up-to-date.";
            }

            return header;
        };
    }

}
