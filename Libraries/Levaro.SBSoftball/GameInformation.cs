using System.Text.Json.Serialization;

using HtmlAgilityPack;

using Levaro.SBSoftball.Common;

namespace Levaro.SBSoftball
{

    /// <summary>
    /// Game meta data (as opposed to stats) for a <see cref="ScheduledGame"/>. It is a property of the <see cref="Game"/>
    /// instance which is the value of <see cref="ScheduledGame.GameResults"/> property.
    /// </summary>
    public sealed class GameInformation
    {
        /// <summary>
        /// Creates an "empty" instanced.
        /// </summary>
        /// <remarks>
        /// This is only used for initialized in other classes (in particular, <see cref="Game"/>). Because all properties
        /// have "init" setters, the class can really only be instantiated using the static 
        /// <see cref="ConstructGameInformation(ScheduledGame, HtmlDocument)"/> method.
        /// </remarks>
        public GameInformation()
        {
            Title = string.Empty;
            GameId = string.Empty;
            LeagueCategory = string.Empty;
            LeagueDay = string.Empty;
            Season = string.Empty;
        }

        /// <summary>
        /// Gets or initializes the <c>Title</c> of the information class.
        /// </summary>
        /// <remarks>
        /// The title is set as "[visitor team name] vs [home team name]".
        /// </remarks>
        public string Title
        {
            get;
            init;
        }

        /// <summary>
        /// Gets or initializes a unique ID for the game
        /// </summary>
        /// <remarks>
        /// The ID is a 5 digit number which is the last segment of the <see cref="DataSource"/> (Uri) property.
        /// </remarks>
        public string GameId
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the <see cref="Uri"/> of the page that contains all the <see cref="Game"/> information.
        /// </summary>
        /// <Remarks>
        /// The string value found in <see cref="ScheduledGame.ResultsUrl"/> is used to produce this value, but is included
        /// for convenience.
        /// </Remarks>
        /// <seealso cref="GameInformation.GameId"/>
        public Uri? DataSource
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the date and time of the game.
        /// </summary>
        /// <remarks>
        /// Again this is the same as in the <see cref="ScheduledGame.Date"/> and is included for convenience when
        /// querying the data store.
        /// </remarks>
        public DateTime Date
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the league category, one of Recreation, Sidewinder, Coyote, Community and Competitive.
        /// </summary>
        public string LeagueCategory
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the league day, one of Monday through Friday
        /// </summary>
        public string LeagueDay
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the league season, one of Winter, Spring, Summer and Fall.
        /// </summary>
        public string Season
        {
            get;
            init;
        }

        /// <summary>
        /// Constructs the <see cref="GameInformation"/> instance for the <see cref="Game.GameInformation"/> property.
        /// </summary>
        /// <remarks>
        /// This method is only called from <see cref="Game"/> class and the only time it is called is when <c>Game</c> is 
        /// instantiated and not when updated. 
        /// </remarks>
        /// <param name="scheduledGame">
        /// The <see cref="ScheduledGame"/> for which the <see cref="ScheduledGame.GameResults"/> property (that is, <c>Game</c>
        /// object) is created.
        /// </param>
        /// <param name="htmlDocument">The <see cref="HtmlDocument"/> for the page having the game data.</param>
        /// <returns>
        /// A <c>GameInformation</c> instance; never <c>null</c>.
        /// </returns>
        public static GameInformation ConstructGameInformation(ScheduledGame scheduledGame, HtmlDocument htmlDocument)
        {
            string title = htmlDocument.DocumentNode.SelectSingleNode("//article/header/h1").InnerHtml.CleanNameText();
            Uri? dataSource = scheduledGame.ResultsUrl;
            string gameId = string.Empty;
            if (dataSource != null)
            {
                string lastSegment = dataSource.Segments.ToList().Last();
                string id = lastSegment;
                if (lastSegment.EndsWith("/"))
                {
                    int length = lastSegment.Length - 1;
                    id = lastSegment[..length];
                }

                gameId = id;
            }

            DateTime date = scheduledGame.Date;

            List<HtmlNode> spSectionContentNodes = htmlDocument.DocumentNode.SelectSingleNode("//article/div").SelectNodes("div").ToList();
            HtmlNode gameDetails = spSectionContentNodes.Where(n => n.HasClass("sp-section-content-details")).Single();
            List<string> details = gameDetails.SelectSingleNode("//tbody/tr").Elements("td").Select(n => n.InnerHtml).ToList();
            string[] league = details[2].Split(' ');
            string leagueCategory = league[1].Trim();
            string leagueDay = league[0].Trim();
            string season = details[3].Split(' ')[0].Trim();

            return new GameInformation()
            {
                Title = title,
                GameId = gameId,
                DataSource = dataSource,
                Date = date,
                LeagueCategory = leagueCategory,
                LeagueDay = leagueDay,
                Season = season,
            };
        }

        /// <summary>
        /// Returns the year of the league in the format "yyyy".
        /// </summary>
        /// <remarks>
        /// This property is not serialized, but it useful for display.
        /// </remarks>
        [JsonIgnore]
        public string Year => $"{Date.Date:yyyy}";

        /// <summary>
        /// Overrides the <see cref="ToString()"/> method to provide a description of the current instance.
        /// </summary>
        /// <returns>
        /// The format is "[[Title]] [[[Season]] [[LeagueDay]] [[LeagueCategory]]] on [[Date:dddd MMMM, d yyyy]]"
        /// where those enclosed in "[[ ]]" are the current values of the properties. For example,
        /// <example>
        /// The Charles Company vs Tucson Orthopaedic Institute [Fall Monday Community] on Monday October, 9 2023
        /// </example>
        /// </returns>
        public override string ToString()
        {
            string description = "No description available";
            if (!string.IsNullOrEmpty(Title))
            {
                description = $"{Title} [{Season} {LeagueDay} {LeagueCategory}] on {Date:dddd MMMM, d yyyy}";
            }

            return description;
        }
    }
}
