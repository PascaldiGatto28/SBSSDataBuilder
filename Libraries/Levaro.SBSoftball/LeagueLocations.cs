using HtmlAgilityPack;

namespace Levaro.SBSoftball
{
    /// <summary>
    /// Encapsulates the page URLs that have the league schedule information.
    /// </summary>
    public sealed class LeagueLocations
    {
        /// <summary>
        /// The private default construction that initializes the two properties to their default values.
        /// </summary>
        /// <remarks>
        /// Instances can only be created using the <see cref="ConstructLeagueLocations(string?)"/> static method because
        /// the only constructor is private and property setters are <c>init</c>. This class is immutable and because the data 
        /// does not change during the league play, it needs only be constructed once if persisted.
        /// </remarks>
        private LeagueLocations()
        {
            Locations = new Dictionary<string, string>();
            BuildDate = DateTime.MinValue;
        }

        /// <summary>
        /// Gets and initializes the time stamp the instance was created. The default constructs initializes this to
        /// <c>DateTime.MinValue</c>, the <see cref="ConstructLeagueLocations(string?)"/> sets the value to current date and
        /// time the instance is created.
        /// </summary>
        public DateTime BuildDate
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the dictionary of league name and URL values (strings) of the pages that contain the complete
        /// league schedule for the league.
        /// </summary>
        /// <remarks>
        /// This property is initialized to the empty dictionary by the default constructed, but dictionary is populated when
        /// the <see cref="ConstructLeagueLocations(string?)"/> static method successfully returns.
        /// </remarks>
        public Dictionary<string, string> Locations
        {
            get;
            init;
        }

        /// <summary>
        /// Creates <see cref="LeagueLocations"/> instance using the <paramref name="saddleBrookeSeniorSoftball"/> property to
        /// scrape the page for the information.
        /// </summary>
        /// <param name="saddleBrookeSeniorSoftball">The optional value that specifies the URL where the league schedules
        /// can be found. If not specified, <c>https://saddlebrookesoftball.com/</c> is used.
        /// </param>
        /// <returns>The created <c>LeagueLocations</c> instance or if the page cannot be found or parsed the instance
        /// created by the default constructor unless an exception is thrown. <c>null</c> is never returned.</returns>
        /// <exception cref="InvalidOperationException">
        /// If the page having the URL (string) <paramref name="saddleBrookeSeniorSoftball"/> could not be parsed (scraped) to
        /// recover the league schedule page URLs.
        /// </exception>
        public static LeagueLocations ConstructLeagueLocations(string? saddleBrookeSeniorSoftball = null)
        {
            LeagueLocations leagues = new();
            Dictionary<string, string> locations = new();
            string url = saddleBrookeSeniorSoftball ?? "https://saddlebrookesoftball.com/";
            try
            {
                HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(new Uri(url));
                IEnumerable<KeyValuePair<string, string>> locationKVPs = htmlDocument.DocumentNode
                                                          .SelectNodes("//ul[@id='menu-homepage-navigation']/li/a")
                                                          .Single(n => n.InnerText == "Schedules")
                                                          .ParentNode
                                                          .SelectNodes("ul/li/a")
                                                          .Select(n => new KeyValuePair<string, string>(n.InnerText.Replace(" Schedule", string.Empty), n.GetAttributeValue("href", string.Empty)));


                foreach (KeyValuePair<string, string> kvp in locationKVPs)
                {
                    locations.Add(kvp.Key, kvp.Value);
                }

                leagues = new()
                {
                    BuildDate = DateTime.Now,
                    Locations = locations
                };
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Failed to recover locations from the URL \"{url}\"", exception);
            }

            return leagues;
        }
    }
}
