// Ignore Spelling: Linq

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
            string text = seasonText;
            string season = seasonText.RemoveWhiteSpace();
            string dataStorePath = $@"{dataStoreFolder}{season}LeaguesData.json";

            Assembly assembly = typeof(LogSessions).Assembly;
            string resName = assembly.FormatResourceName("DataStoreInfo.html");
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();

            string changedHtml = string.Empty;

            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath))
            {
                DataStoreInformation dsInfo = new DataStoreInformation(dsContainer ?? DataStoreContainer.Empty);
                using (HtmlGenerator generator = new HtmlGenerator())
                {

                    generator.WriteRootTable(dsInfo, LinqPadCallbacks.ExtendedDsInfo(headerStyle));

                    Values.Add(dsInfo);
                    if (callback != null)
                    {
                        callback(this);
                    }

                    string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    changedHtml = generator.DumpHtml(pageTitle: title,
                                                     cssStyles: StaticConstants.LocalStyles,
                                                     javaScript: StaticConstants.LocalJavascript,
                                                     collapseTo: 1,
                                                     headElements: headElements.ToList());
                }
            }

            return changedHtml;
        }
    }


}
