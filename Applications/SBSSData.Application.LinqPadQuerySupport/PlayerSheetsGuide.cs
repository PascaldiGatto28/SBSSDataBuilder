// Ignore Spelling: Linq

using SBSSData.Application.Support;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class PlayerSheetsGuide : IHtmlCreator
    {
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "A guide to the Player Sheets page"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["Player League Summaries", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["../href", "SBSSData.ico"]])
        };

        public PlayerSheetsGuide()
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
            PlayerSheets playerSheetsGuide = new PlayerSheets("PlayerSheetsContainerGuide.html");
            string html =  playerSheetsGuide.BuildHtmlPage(seasonText, dataStoreFolder, null);
            if (callback != null)
            {
                callback(this);
            }

            return html;
        }
    }
}
