using HtmlAgilityPack;

using SBSSData.Softball.Common;

namespace SBSSData.Softball
{
    /// <summary>
    /// Encapsulates meta-information about a league which is encapsulated in the <see cref="LeagueSchedule"/> class when it's
    /// constructed.
    /// </summary>
    /// <remarks>
    /// When a <see cref="LeagueSchedule"/> is created by the static method 
    /// <see cref="LeagueSchedule.ConstructLeagueSchedule(string)"/> a description of the league including the league schedule
    /// page is also constructed. This class is often used for reporting purposes. Instances are only constructed via the
    /// static method or JSON deserialization.
    /// <para>
    /// Because the default constructor is private and all property setters are "private init", instance of this class are
    /// essentially immutable and typically can only be created using the static
    /// <see cref="ConstructionLeagueDescription(Uri, HtmlDocument)"/> method.
    /// </para>
    /// </remarks>
    public sealed class LeagueDescription
    {
        /// <summary>
        /// Creates an "empty" instance. 
        /// </summary>
        public LeagueDescription()
        {
            LeagueCategory = string.Empty;
            LeagueDay = string.Empty;
            Season = string.Empty;
            Year = string.Empty;
        }

        /// <summary>
        /// Returns and initializes the league category.
        /// </summary>
        /// <remarks>
        /// The league categories are one of "Recreation", "SideWinder", "Coyote", "Community" or "Competitive".
        /// </remarks>
        public string LeagueCategory
        {
            get;
            init;
        }

        /// <summary>
        /// Returns and initializes the league day, which is Monday through Friday.
        /// </summary>
        public string LeagueDay
        {
            get;
            init;
        }

        /// <summary>
        /// Returns and initializes the season, which is Spring, Summer, Fall and Winter.
        /// </summary>
        public string Season
        {
            get;
            init;
        }

        /// <summary>
        /// Returns and initializes the year the league games started, for example 2023.
        /// </summary>
        public string Year
        {
            get;
            init;
        }

        /// <summary>
        /// Returns and initializes the <see cref="Uri"/> of the page containing all league schedule information.
        /// </summary>
        public Uri? ScheduleDataSource
        {
            get;
            init;
        }

        /// <summary>
        /// Constructs a <c>LeagueDescription</c> object. This method is the only way to create an instance of the
        /// <see cref="LeagueDescription"/> class.
        /// </summary>
        /// <remarks>
        /// Generally there is no reason to call this method directly, because the static
        /// <see cref="LeagueSchedule.ConstructLeagueSchedule(string)"/> method calls it when creating a 
        /// <see cref="LeagueSchedule"/> instance and stores the instance in its <see cref="LeagueSchedule.LeagueDescription"/>
        /// property.
        /// </remarks>
        /// <param name="scheduleDataSource">The <see cref="Uri"/> to the page containing the data to build the instance. The
        /// value should be value of the <see cref="LeagueLocations.Locations"/> dictionary property.</param>
        /// <param name="htmlDocument">The <see cref="HtmlDocument"/> returned by the 
        /// <see cref="PageContentUtilities.GetPageHtmlDocument(Uri)"/>.
        /// </param>
        /// <returns>A <c>LeagueDescription</c> instance; it is never <c>null</c>.</returns>
        /// <exception cref="InvalidOperationException">
        /// if the page recovered from specified <paramref name="scheduleDataSource"/> cannot be parsed to provide the 
        /// class data.
        /// </exception>
        /// <seealso cref="LeagueSchedule"/>
        /// <seealso cref="PageContentUtilities.GetPageHtmlDocument(Uri)"/>
        public static LeagueDescription ConstructionLeagueDescription(Uri scheduleDataSource, HtmlDocument htmlDocument)
        {
            try
            {
                HtmlNode article = htmlDocument.DocumentNode.SelectSingleNode("//article");
                string articleClass = article.GetAttributeValue("class", string.Empty);
                string[] leagueInfo = articleClass.Substring("sp_league-", "-league", false, false)
                                                  .Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                string leagueDay = leagueInfo[0].Trim().Capitalize();
                string leagueCategory = leagueInfo[1].Trim().Capitalize();


                string[] leagueSeason = articleClass.Substring("sp_season", false)
                                                    .Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                string season = leagueSeason[0].Trim().Capitalize();
                string year = leagueSeason[1].Trim();

                return new LeagueDescription()
                {
                    LeagueCategory = leagueCategory,
                    LeagueDay = leagueDay,
                    Season = season,
                    Year = year,
                    ScheduleDataSource = scheduleDataSource
                };
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException($"Could not parse the {scheduleDataSource} page.", exception);
            }
        }

        /// <summary>
        /// Gets the "empty" <see cref="LeagueDescription"/> object.
        /// </summary>
        /// <returns>The <see cref="LeagueDescription"/> object having all properties set to their default values.</returns>
        public static LeagueDescription Empty()
        {
            return new LeagueDescription();
        }

        /// <summary>
        /// Overrides the default <see cref="Object.ToString()"/> method.
        /// </summary>
        /// <returns>Returns a description string containing the values of <c>string</c> properties. For example,
        /// "Monday Recreation Fall 2023"</returns>
        public override string ToString()
        {
            return $"{LeagueDay} {LeagueCategory} {Season} {Year}";
        }

        public string ToShortString()
        {
            return $"{LeagueDay} {LeagueCategory}";
        }

        /// <summary>
        /// Overrides the default <see cref="Object.Equals(object?)"/> method.
        /// </summary>
        /// <param name="obj">The <c>object</c> to test for equality.</param>
        /// <returns>Returns <c>true</c> if <paramref name="obj"/> is not null of type <see cref="LeagueDescription"/> and
        /// <c>string</c> property values are equal (case insensitive).</returns>
        public override bool Equals(object? obj)
        {
            bool isEqual = false;
            if ((obj != null) && (GetType() == obj.GetType()))
            {
                LeagueDescription description = (LeagueDescription)obj;
                isEqual = string.Equals(LeagueCategory, description.LeagueCategory, StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(LeagueDay, description.LeagueDay, StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(Season, description.Season, StringComparison.OrdinalIgnoreCase) &&
                          string.Equals(Year, description.Year, StringComparison.OrdinalIgnoreCase);

            }

            return isEqual;
        }

        /// <summary>
        /// Overrides the default <see cref="Object.GetHashCode()"/> method
        /// </summary>
        /// <returns>
        /// Returns the hash code of the string returned by the <see cref="LeagueDescription.ToString()"/> method.
        /// </returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}
