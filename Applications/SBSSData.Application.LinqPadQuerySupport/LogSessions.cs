// Ignore Spelling: Linq

using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Logging;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class LogSessions
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
            new HeadElement("meta", [["name", "author"], ["content", "Pascal diGatto"]]),
            new HeadElement("meta", [["data", "description"], ["content", "Log session details when the data store is updated"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["name", "http-equiv"], ["content", "no-cache"]]),
            new HeadElement("title", [["Data Store Log", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "SBSSData.ico"]])
        };

        private static string headerStyle = "background-color:#d62929;";

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            string text = seasonText;
            string season = seasonText.RemoveWhiteSpace();
            string dataStorePath = $@"{dataStoreFolder}{season}LeaguesData.json";

            Assembly assembly = typeof(GamesTeamPlayersV3).Assembly;
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
                    l.Date,
                    //Category = l.LogCategory,
                    Text = l.LogText
                })
            });

            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath))
            {
                DataStoreInformation dsInfo = new DataStoreInformation(dsContainer ?? DataStoreContainer.Empty);
                using (HtmlGenerator generator = new HtmlGenerator())
                {

                    generator.WriteRootTable(dsInfo, LinqPadCallbacks.ExtendedDsInfo(headerStyle));

                    Values.Add(dsInfo);
                    if (callback != null)
                    {
                        callback(dsInfo);
                    }

                    generator.WriteRootTable(displaySessions, LogSessionsCallback);
                    Values.Add(dsInfo);
                    if ((callback != null) && (displaySessions != null))
                    {
                        callback($"{displaySessions.Count()} log sessions found");
                    }
                   
                    string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    changedHtml = generator.DumpHtml(pageTitle: title,
                                                     cssStyles: StaticConstants.LocalStyles,
                                                     javaScript: StaticConstants.LocalJavascript,
                                                     collapseTo: 2,
                                                     headElements: headElements.ToList());
                }
            }

            return changedHtml;
        }

        public static Func<TableNode, string> LogSessionsCallback = (t) =>
        {
            string header = string.Empty;
            if (t.Depth() == 0)
            {
                int numSessions = t.ChildNodes.Count();
                HtmlNode tableNode = t.TableHtmlNode;
                //string currentDate = DateTime.Parse(tableNode.SelectSingleNode("./tbody/tr[1]/td[1]").InnerText).ToString("dddd MMMM d, yyyy");
                header = $"{numSessions} Recorded Log Sessions"; // as of {currentDate}";
                InsertTableDescription(tableNode, "Table of all Data Store Updates Recorded by the SBSS Data Viewer Logging System");
                tableNode.SelectSingleNode("./thead/tr/td").Attributes.Add("style", headerStyle);
                tableNode.SelectSingleNode("./thead/tr[last()]").Attributes.Add("style", "display:none");
            }
            else
            {
                IEnumerable<string> textCells = t.TableHtmlNode.SelectNodes("./tbody//tr/td[2]").Select(n => n.InnerText);
                string? cell = textCells.SingleOrDefault(t => t.Contains("games have been updated"));
                int numUpdated = 0;
                if (!string.IsNullOrEmpty(cell))
                {
                    int index = cell.IndexOf(' ');
                    int.TryParse(cell.Substring(0, index), out numUpdated);
                }

                header = (numUpdated > 0) ? $"{numUpdated.NumDesc("Scheduled Game")} Updated"
                                          : "No Scheduled Games Were Updated &mdash; The Data Store is Up-to-date.";
            }

            return header;
        };
        public static void InsertTableDescription(HtmlNode tableHtmlNode, string description)
        {
            HtmlNode tableTitle = HtmlNode.CreateNode($"""<h2 style="font-size:1.1em; color:#3d5e8f; margin-top:20px; font-weight:500;">{description}</h2>""");
            tableHtmlNode.ParentNode.InsertBefore(tableTitle, tableHtmlNode);
        }
    }

}
