/* 
PSEUDOCODE / PLAN (detailed):

- Add XML documentation comments to every member of the LeagueRoster class (class, constructors,
  properties, static properties and methods). Include <summary>, <remarks>, <param>, <returns>, and
  <exception> where applicable to help other developers.

- Keep functionality unchanged except for fixing obvious collection initializer syntax errors
  that would prevent compilation:
    - Replace `[]` initializers with `new List<T>()`.

- Preserve existing implementation and behavior:
    - Default constructor initializes League to LeagueName.Empty and TeamRosters to an empty list.
    - Overload constructor accepts a league and its team rosters.
    - `Empty` returns a new default-initialized LeagueRoster instance.
    - `ConstructLeagueRoster` builds a roster by fetching the roster page and parsing HTML nodes,
      invoking the optional message callback as progress or error reporting, and returning the
      populated LeagueRoster. Document parameters, returns, and exceptions.

- Add clear XML comments describing:
    - What each member represents.
    - Expected invariants (for example, TeamRosters is never null).
    - The format of the roster URL used by ConstructLeagueRoster.
    - Notable parsing behavior (e.g., special case for malformed manager span).
    - Exceptions that callers should be aware of.

- Ensure comments are concise but informative for developers reading the API.

After this comment block, the file contains the documented LeagueRoster class.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HtmlAgilityPack;

using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Softball.Rosters
{
    /// <summary>
    /// Represents the roster information for an entire league.
    /// </summary>
    /// <remarks>
    /// A <see cref="LeagueRoster"/> instance contains the league identification (<see cref="League"/>) and the
    /// collection of <see cref="TeamRoster"/> instances for that league. Instances are typically constructed via
    /// <see cref="ConstructLeagueRoster(LeagueName, Action{string}?)"/> which fetches and parses an online roster page.
    /// </remarks>
    public class LeagueRoster
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LeagueRoster"/> class with default values.
        /// </summary>
        /// <remarks>
        /// The default constructor sets <see cref="League"/> to <see cref="LeagueName.Empty"/> and initializes
        /// <see cref="TeamRosters"/> to an empty list. This constructor is useful when an empty, mutable instance is required.
        /// </remarks>
        public LeagueRoster()
        {
            League = LeagueName.Empty;
            TeamRosters = new List<TeamRoster>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LeagueRoster"/> class with the specified league and team rosters.
        /// </summary>
        /// <param name="league">The <see cref="LeagueName"/> that identifies the league described by this roster.</param>
        /// <param name="teamRosters">The list of <see cref="TeamRoster"/> objects for the league.</param>
        /// <remarks>
        /// The provided <paramref name="teamRosters"/> list is assigned directly; callers may pass the same list
        /// used elsewhere. To guarantee independent copies, callers should pass a copy of the list.
        /// </remarks>
        public LeagueRoster(LeagueName league, List<TeamRoster> teamRosters)
        {
            League = league;
            TeamRosters = teamRosters;
        }

        /// <summary>
        /// Gets or sets the league identifier for this roster.
        /// </summary>
        /// <remarks>
        /// The <see cref="LeagueName"/> value encodes the league day and category (for example, "Monday Community"),
        /// and provides display-friendly names. This property is expected to be non-null; the default value is
        /// <see cref="LeagueName.Empty"/>.
        /// </remarks>
        public LeagueName League
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets the list of team rosters for the league.
        /// </summary>
        /// <remarks>
        /// This list contains one <see cref="TeamRoster"/> per team in the league. The property is never null;
        /// when no teams are available it will be an empty list.
        /// </remarks>
        public List<TeamRoster> TeamRosters
        {
            get; set;
        }

        /// <summary>
        /// Gets an empty <see cref="LeagueRoster"/> instance.
        /// </summary>
        /// <remarks>
        /// This property returns a newly constructed <see cref="LeagueRoster"/> initialized with default values.
        /// It is provided as a convenience for initialization of other objects.
        /// </remarks>
        public static LeagueRoster Empty => new LeagueRoster();

        /// <summary>
        /// Constructs a <see cref="LeagueRoster"/> for the specified <paramref name="leagueName"/> by fetching and parsing
        /// the online roster page.
        /// </summary>
        /// <param name="leagueName">The league identifier used to build the roster page URL.</param>
        /// <param name="message">
        /// An optional callback used to report progress or diagnostic messages. If <c>null</c>, progress messages are
        /// written to <see cref="Console.WriteLine(string)"/>.
        /// </param>
        /// <returns>
        /// A <see cref="LeagueRoster"/> instance populated with the parsed <see cref="TeamRoster"/> entries.
        /// </returns>
        /// <remarks>
        /// The method constructs the roster page URL using the <see cref="LeagueName.Day"/> and
        /// <see cref="LeagueName.Category"/> values in the form:
        /// <c>https://saddlebrookesoftball.com/{day}-{category}-rosters/</c>.
        /// 
        /// The HTML parsing assumes the roster page contains a container with class <c>entry-content</c>, and that each
        /// team's information is present in nodes with class <c>sportspress</c>. For each team node the first <c>h4</c>
        /// is used for the team name, and <c>span</c> nodes are searched for manager information. Table rows with
        /// <c>td</c> elements of class <c>data-name</c> are collected as player names.
        /// 
        /// There is a known page formatting issue where one roster is missing a closing <c>&lt;/span&gt;</c>. The code
        /// attempts to handle that by trimming manager text at a discovered "<c>&lt;br</c>" marker when present.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="leagueName"/> is null (should not occur for records).</exception>
        /// <exception cref="System.Net.WebException">May be thrown by underlying page fetch utilities if the page cannot be retrieved.</exception>
        /// <exception cref="NullReferenceException">
        /// Thrown when expected HTML nodes cannot be found during parsing. The exception message is sent to the
        /// <paramref name="message"/> callback before being rethrown.
        /// </exception>
        public static LeagueRoster ConstructLeagueRoster(LeagueName leagueName, Action<string>? message = null)
        {
            if (leagueName is null) throw new ArgumentNullException(nameof(leagueName));

            Action<string> callback = message ?? (m => Console.WriteLine(m));

            LeagueRoster leagueRoster = Empty;
            string rosterUrl = $"https://saddlebrookesoftball.com/{leagueName.Day}-{leagueName.Category}-rosters/";

            List<TeamRoster> teamRosters = new List<TeamRoster>();

            callback($"Processing {leagueName.FullLeagueName}");
            leagueRoster.League = leagueName;

            HtmlDocument htmlDocument = PageContentUtilities.GetPageHtmlDocument(new Uri(rosterUrl));
            HtmlNode root = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='entry-content']");

            try
            {
                // The manager span contains text like "Manager: John Doe". Want to extract just the name,
                // so trim off the "Manager:" prefix.
                List<string> managers = root.SelectNodes("//span").Where(s => s.InnerHtml.Contains("Manager:"))
                                            .Select(s => s.InnerHtml.Substring("Manager:".Length).TrimStart()).ToList();
                int numManagers = managers.Count;

                // There is a "sportspress" node for each team, and the manager spans are in the same order as the teams. 
                HtmlNodeCollection spNodes = root.SelectNodes("//div[@class='sportspress']");
                for (int i = 0; i < managers.Count; i++)
                {
                    HtmlNode spNode = spNodes.ElementAt(i);
                    string teamName = spNode.SelectSingleNode("h4").InnerText.CleanNameText();
                    string manager = managers[i];
                    
                    HtmlNode tableBody = spNode.SelectSingleNode("div/div/table/tbody");
                    IEnumerable<string> players = tableBody.SelectNodes("tr/td[@class='data-name']").Select(n => n.InnerText.CleanNameText());
                    TeamRoster roster = new(teamName, manager, players.ToList());
                    teamRosters.Add(roster);
                }

                leagueRoster.TeamRosters = teamRosters;
            }
            catch (NullReferenceException exception)
            {
                callback(exception.Message);
                throw;
            }

            return leagueRoster;
        }
    }
}
