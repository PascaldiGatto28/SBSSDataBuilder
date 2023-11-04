using HtmlAgilityPack;

namespace Levaro.SBSoftball
{
    public class LeagueLocations
    {
        public LeagueLocations()
        {
            Locations = new Dictionary<string, string>();
        }

        public DateTime BuildDate
        {
            get;
            set;
        }

        public Dictionary<string, string> Locations
        {
            get;
            set;
        }

        public static LeagueLocations ConstructLeagueLocations(string? saddlebrookeSeniorSoftball = null)
        {
            string json = string.Empty;
            Dictionary<string, string> locations = new Dictionary<string, string>();
            string url = saddlebrookeSeniorSoftball ?? "https://saddlebrookesoftball.com/";
            HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(new Uri(url));
            IEnumerable<KeyValuePair<string, string>> locationKVPs = htmlDocument.DocumentNode
                                                      .SelectNodes("//ul[@id='menu-homepage-navigation']/li/a")
                                                      .Single(n => n.InnerText == "Schedules")
                                                      .ParentNode
                                                      .SelectNodes("ul/li/a")
                                                      .Select(n => new KeyValuePair<string, string>(n.InnerText.Replace(" Schedule", string.Empty), n.GetAttributeValue("href", string.Empty)));
            LeagueLocations leagues = new LeagueLocations
            {
                BuildDate = DateTime.Now
            };

            foreach (KeyValuePair<string, string> kvp in locationKVPs)
            {
                locations.Add(kvp.Key, kvp.Value);
            }

            leagues.Locations = locations;


            return leagues;
        }
    }
}
