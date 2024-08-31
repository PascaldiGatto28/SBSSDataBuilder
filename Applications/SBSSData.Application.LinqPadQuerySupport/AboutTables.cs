using System.Reflection;

using HtmlAgilityPack;

using SBSSData.Application.Support;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class AboutTables : IHtmlCreator
    {

        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "Description of Data Viewer Taables"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["About Tables", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        private static string headerStyle = "background-color:#d62929;";
        private static string fixedSeasonText = "2023 Summer";
        private static string teamName = "Hardin Brothers";
        private static string league = "Recreation";
        private static string day = "Monday";
        private static string photosPath = "./PlayerPhotos/";

        private static List<string> htmlInfo = [
        "Casey Stengel &mdash; The secret of successful managing is to keep the five players who hate you away from the four players who haven't made up their minds.",
        "Casey Stengel &mdash; All right everyone, line up alphabetically according to your height.",
        "Casey Stengel &mdash; Good pitching will always stop good hitting and vice-versa.",
        "Casey Stengel &mdash; Can't anybody here play this game?",
        "Casey Stengel &mdash; The trick is growing up without growing old.",
        "Yogi Berra &mdash; It ain't over till it's over.",
        "Yogi Berra &mdash; When you come to a fork in the road, take it.",
        "Yogi Berra &mdash; You can observe a lot by just watching.",
        "Yogi Berra &mdash; Baseball is 90% mental and the other half is physical.",
        "Yogi Berra &mdash; Always go to other people's funerals, otherwise they won't come to yours",
        "Yogi Berra &mdash; This is like Deja vu all over again.",
        "Reggie Jackson &mdash; Fans don't boo nobodies.",
        "Mickey Mantle &mdash; It's unbelievable how much you don't know about the game you've been playing your whole life.",
        "Willie Stargell &mdash; They give you a round bat, and they throw you a round ball, and then they tell you to hit it square.",
        "Pascal diGatto &mdash; Slump? I'm not in a slump. I'm just not hitting.",
        "Willie Mays &mdash; For all its gentility, its almost leisurely pace, baseball is violence under wraps.",
        "Yogi Berra &mdash; If the people don't wanna come out to the ballpark, nobody's gonna stop 'em.",
        "Willie Stargell &mdash; When you start the game, they don't say 'Work Ball!' They say, 'Play ball!",
        "Tony Gwynn &mdash; Remember these two things: play hard and have fun."
        ];

        public AboutTables()
        {
            Values = [];
            ResourceName = $"{this.GetType().Name}.html";
        }

        public List<object> Values
        {
            get;
            set;
        }

        public string ResourceName
        {
            get;
            set;
        }

        public string BuildHtmlPage(string seasonText,
                                    string dataStoreFolder,
                                    Action<object>? callback = null)
        {

            Action<object>? actionCallback = callback;
            string season = fixedSeasonText.RemoveWhiteSpace();

            string changedHtml = string.Empty;

            Assembly assembly = typeof(AboutTables).Assembly;
            string resName = assembly.FormatResourceName(ResourceName);
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();

            string path = $"{dataStoreFolder}{season}LeaguesData.json";

            using DataStoreContainer dsContainer = DataStoreContainer.Instance(path);
            Query query = new(dsContainer);
            List<PlayerStatsDisplay> psd = query.GetTeamPlayersStats(teamName, league, day)
                                                .Players
                                                .Cast<PlayerStats>()
                                                .Select(p => new PlayerStatsDisplay(p))
                                                .ToList();

            using HtmlGenerator generator = new();

            generator.WriteRootTable(psd, ExtendedPSDTable(headerStyle, $"{dataStoreFolder}PlayerPhotos/"));

            string htmlNode = $"<div>{html.Substring("<div class=\"IntroContent\"", "</body>", true, false)}</div>";
            HtmlNode title = HtmlNode.CreateNode(htmlNode);
            changedHtml = generator.DumpHtml(pageTitle: title,

                                             cssStyles: StaticConstants.LocalStyles + 
                                                        StaticConstants.HelpStyles +
                                                        StaticConstants.SortableTableStyles +
                                                        StaticConstants.AboutTablesStyles,
                                                        
                                             javaScript: StaticConstants.LocalJavascript,
                                             collapseTo: 1,
                                             headElements: headElements.ToList());

            if (callback != null)
            {
                callback($"{this.GetType().Name} page created.");
            }

            //HtmlDocument htmlDoc = new();
            //htmlDoc = PageContentUtilities.GetHtmlDocument(containerHtml);
            //HtmlNode root = htmlDoc.DocumentNode;
            //HtmlNode sampleTableFrame = root.SelectSingleNode("//iframe[@id='sampleTable']");
            //sampleTableFrame.SetAttributeValue("style", "width:680px; height:525px;");
            //sampleTableFrame.SetAttributeValue("srcdoc", changedHtml);

            return changedHtml;
        }

        public static Func<TableNode, string> ExtendedPSDTable(string? headerCssStyle = null,
                                                               object? value = null)
        {
            return (t) =>
            {
                string header = string.Empty;
                if (!t.Id().StartsWith("doc"))
                {
                    HtmlNode tableHtmlNode = t.TableHtmlNode;
                    HtmlNode divNode = tableHtmlNode.ParentNode;
                    divNode.SetAttributeValue("style", "margin:0px; padding:0px;");
                    //divNode.ParentNode.SetAttributeValue("style", "margin:0px; padding:0px;");
                    Utilities.ConvertToSortable(tableHtmlNode, [1], true, 3);
                    string photoPath = (value as string) ?? ".PlayerPhotos/";

                    IEnumerable<HtmlNode> rows = [.. tableHtmlNode.SelectNodes("./tbody//tr")];

                    int quoteNumber = 0;
                    foreach (HtmlNode row in rows)
                    {
                        HtmlNode nameCell = row.SelectSingleNode("./td[2]");
                        nameCell.SetAttributeValue("style", "cursor:pointer");

                        string playerPhotoName = Utilities.GetPlayerPhotoName(nameCell.InnerText);
                        nameCell.SetAttributeValue("name", playerPhotoName);

                        string overlayStyle = """
                                              text-align:center; 
                                              left:400px; top:50px; 
                                              width:162px; 
                                              justify-content:center;
                                              """;
                        string overlayHtml = StaticConstants.BuildGenericOverlay(overlayStyle,
                                                                                 photosPath,
                                                                                 playerPhotoName,
                                                                                 htmlInfo[quoteNumber++]);

                        HtmlNode popUp = HtmlNode.CreateNode(overlayHtml);
                        nameCell.ChildNodes.Append(popUp);
                        nameCell.Attributes.Add("onmouseover", "this.querySelector('.overlay').style.display='block'");
                        nameCell.Attributes.Add("onmouseout", "this.querySelector('.overlay').style.display='none'");
                    }

                    header = $"Summary Stats for all {rows.Count()} {teamName} Players for {fixedSeasonText} {day} {league}";
                }

                return header;
            };
        }
    }
}
