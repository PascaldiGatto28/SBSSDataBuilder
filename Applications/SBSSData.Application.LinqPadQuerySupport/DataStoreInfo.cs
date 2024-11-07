// Ignore Spelling: Linq Css

using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class DataStoreInfo : IHtmlCreator
    {
        public DataStoreInfo()
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
            new HeadElement("meta", [["data", "description"], ["content", "Data Store information"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["Data Store Information", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        private static string headerStyle = "background-color:#d62929;";

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            Assembly assembly = typeof(LogSessions).Assembly;
            string resName = assembly.FormatResourceName("DataStoreInfo.html");
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();

            string changedHtml = string.Empty;


            // There is a gotcha here. At this point, the current DataStoreContainer (dsc) is the
            // the last built data store and if the instance is non-null that dsc will be used. That
            // works fine for the values that are updated like played games, but not players because the
            // property is not updated during the update process. So we need to either call the GetPlayers
            // method directly or better yet, just set the current instance to null. This will force 
            // the data store to be read from file. Like all the IHtmlCreater classes, the instance is
            // created from the data store file.
            DataStoreContainer.Empty.Dispose();  // The instance is now null.

            List<DSInformationDisplay> dsInfoList = [];
            foreach (string season in StaticConstants.Seasons)
            {
                string dataStorePath = $@"{dataStoreFolder}{season.RemoveWhiteSpace()}LeaguesData.json";

                // Each iteration will load the data store from its json file, because it is disposed 
                // the container is disposed.
                using (DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath))
                {
                    dsInfoList.Add(new DSInformationDisplay(season, dsContainer));
                }
            }

            using (HtmlGenerator generator = new HtmlGenerator())
            {
                generator.WriteRootTable(dsInfoList, ExtendedDSInformationDisplay(headerStyle));

                Values.Add(dsInfoList);
                if (callback != null)
                {
                    callback($"{this.GetType().Name} HTML page created.");
                }

                string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                HtmlNode title = HtmlNode.CreateNode(htmlNode);
                changedHtml = generator.DumpHtml(pageTitle: title,
                                                 cssStyles: StaticConstants.LocalStyles,
                                                 javaScript: StaticConstants.LocalJavascript,
                                                 collapseTo: 1,
                                                 headElements: headElements.ToList());
            }

            return changedHtml;
        }

        public static Func<TableNode, string> ExtendedDSInformationDisplay(string? headerCssStyle = null, object? value = null)
        {
            return (t) =>
            {
                HtmlNode tableHtmlNode = t.TableHtmlNode;
                int numSeasons = tableHtmlNode.SelectNodes("./tbody//tr").Count;
                string header = $"Data Store Information for all {numSeasons} Seasons";
                if (!string.IsNullOrWhiteSpace(headerCssStyle))
                {
                    tableHtmlNode.SelectSingleNode("./thead/tr/td").Attributes.Add("style", headerCssStyle);
                }

                HtmlNodeCollection tableColumnHeaders = tableHtmlNode.SelectNodes("./thead/tr[2]//th");

                //foreach (HtmlNode th in tableColumnHeaders)
                for (int i = 0; i < tableColumnHeaders.Count; i++)
                {
                    HtmlNode th = tableColumnHeaders[i];
                    string columnHeaderTitle = th.GetAttributeValue("title", string.Empty);
                    if ((columnHeaderTitle == "System.Int32") && th.InnerHtml.Contains("&nbsp;"))
                    {
                        th.InnerHtml = th.InnerHtml.Replace("&nbsp;", "<br/>");
                        th.SetAttributeValue("style", "text-align:center");
                    }
                    else
                    {
                        th.SetAttributeValue("style", "vertical-align:middle");
                    }

                    tableColumnHeaders[i].SetAttributeValue("title", titles[i]);
                }

                return header;
            };
        }

        public static readonly List<string> titles = 
                          ["Calendar season followed by year",
                           "When the Data Store was last updated and saved",
                           "Number of leagues in the season",
                           "The size of the Data Store on disk",
                           "Number of scheduled games",
                           "Number of Recorded games (when box scores are recorded) - It is sum of played, canceled and forfeited games",
                           "Scheduled games that were played",
                           "Scheduled games that were canceled",
                           "Scheduled games that were forfeited",
                           "Number of teams that have participated in at least one game",
                           "Number of players who have played in at least one game"];
    }


}
