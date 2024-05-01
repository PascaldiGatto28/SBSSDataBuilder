// Ignore Spelling: Linq

using HtmlAgilityPack;

using SBSSData.Application.Support;


namespace SBSSData.Application.LinqPadQuerySupport
{
    public class GamesTeamPlayersHelpV3 : IHtmlCreator
    {
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "Guide to games, teams and players"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["name", "http-equiv"], ["content", "no-cache"]]),
            new HeadElement("title", [["Guide to Games,Teams & Players", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        public GamesTeamPlayersHelpV3()
        {
            Values = [];
        }

        public List<object> Values
        {
            get;
            set;
        }

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            GamesTeamPlayersV3  gtpV3 = new GamesTeamPlayersV3();
            gtpV3.ResourceName = $"{this.GetType().Name}.html";

            string html = gtpV3.BuildHtmlPage(seasonText, dataStoreFolder, callback);
            string changedHtml = html;

            // Add meta, link and title elements to the document head for this page.
            HtmlDocument htmlDocument = new();
            htmlDocument.LoadHtml(html);
            HtmlNode rootNode = htmlDocument.DocumentNode;
            HtmlNode headNode = rootNode.SelectSingleNode("//head");
            HtmlGenerator.AddHeadData(htmlDocument, headNode, GamesTeamPlayersHelpV3.headElements.ToList());

            List<HtmlNode> nodesToRemove = [];
            HtmlNodeCollection tableNodes = rootNode.SelectNodes("//body//div[@class='spacer']/table");
            if ((tableNodes != null) && (tableNodes.Count > 1))
            {
                for (int i = 1; i < tableNodes.Count; i++)
                {
                    nodesToRemove.Add(tableNodes[i]);
                }

                foreach (HtmlNode node in nodesToRemove)
                {
                    node.Remove();
                }
            }

            changedHtml = rootNode.OuterHtml;
            return changedHtml;
        }
    }
}