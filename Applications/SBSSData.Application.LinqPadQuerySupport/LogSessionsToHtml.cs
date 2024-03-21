// Ignore Spelling: Linq

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Logging;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class LogSessionsToHtml
    {
        public LogSessionsToHtml()
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
            new HeadElement("title", [["Update Log", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "SBSSData.ico"]])
        };

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            string text = seasonText;
            string season = seasonText.RemoveWhiteSpace();
            string dataStorePath = $@"{dataStoreFolder}{season}LeaguesData.json";

            string changedHtml = string.Empty;

            string logPath = $@"{dataStoreFolder}DataStoreManager.json";
            IEnumerable<LogSession>? sessions = logPath.Deserialize<IEnumerable<LogSession>>();

            var displaySessions = sessions?.Select(s => new
            {
                s.BuildDate,
                SessionID = s.Session,
                LogEntries = s.LogEntries.Select(l => new
                {
                    l.Date,
                    Category = l.LogCategory,
                    Text = l.LogText
                })
            });

            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath))
            {
                DataStoreInformation dsInfo = new DataStoreInformation(dsContainer ?? DataStoreContainer.Empty);
                using (HtmlGenerator generator = new HtmlGenerator())
                {
                    generator.WriteRootTable(dsInfo, (t) =>
                    {
                        HtmlNode tableNode = t.TableHtmlNode;
                        InsertTableDescription(t.TableHtmlNode, $"The Current Data Store Statistics for the {seasonText} Season");
                        return "Data Store Information";
                    });
                    Values.Add(dsInfo);
                    if (callback != null)
                    {
                        callback(dsInfo);
                    }

                    generator.WriteRootTable(displaySessions, LogSessionsCallback);
                    Values.Add(dsInfo);
                    if ((callback != null) && (displaySessions != null))
                    {
                        callback(displaySessions);
                    }

                    string htmlNode = """<span style="color:firebrick; font-size:1.50em; font-weight:500;">Log Sessions</span>""";
                    //string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    changedHtml = generator.DumpHtml(pageTitle: title, collapseTo: 2, headElements: headElements.ToList());
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
                string currentDate = DateTime.Parse(tableNode.SelectSingleNode("./tbody/tr[1]/td[1]").InnerText).ToString("dddd MMMM d, yyyy");
                header = $"{numSessions} Recorded Log Sessions as of {currentDate}";
                InsertTableDescription(tableNode, "Table of all Data Store Updates Recorded by the SBSS Logging System");
            }
            else
            {
                IEnumerable<string> textCells = t.TableHtmlNode.SelectNodes("./tbody//tr/td[3]").Select(n => n.InnerText);
                string? cell = textCells.SingleOrDefault(t => t.Contains("games have been updated"));
                int numUpdated = 0;
                if (!string.IsNullOrEmpty(cell))
                {
                    int index = cell.IndexOf(' ');
                    int.TryParse(cell.Substring(0, index), out numUpdated);
                }

                header = (numUpdated > 0) ? $"{numUpdated.NumDesc("Scheduled Game")} Updated"
                                          : "No Scheduled Games Were Updated &mdash; tThe Data Store is Up-to-date.";
            }

            return header;
        };
        public static void InsertTableDescription(HtmlNode tableHtmlNode, string description)
        {
            HtmlNode tableTitle = HtmlNode.CreateNode($"""<h1 class="headingpresenter">{description}</h1>""");
            tableHtmlNode.ParentNode.InsertBefore(tableTitle, tableHtmlNode);
        }
    }

}
