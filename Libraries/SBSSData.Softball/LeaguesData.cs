namespace SBSSData.Softball
{
    /// <summary>
    /// This class is the main class for the SBSS data store. 
    /// </summary>
    /// <remarks>
    /// The sequence of <see cref="LeagueSchedule"/> objects contains all
    /// information for scheduled (completed or not) games for that league. The data are returned from the HTML page whose address 
    /// is provided via a <see cref="LeagueLocations"/> instance. To produce the full data store, iterate over the locations 
    /// (really Urls) to recover a page where the scheduled games for that league can be scraped from the page. The static method 
    /// <see cref="LeagueSchedule.ConstructLeagueSchedule(string)"/> returns the schedule for that league 
    /// and can be added to the <see cref="LeagueSchedules"/> property of this instance.
    /// <para>For example, the <see cref="LeaguesData"/> instance is created and then serialized to JSON file.
    /// <code language="c#" title="Sample Code to Populate the LeagueSchedule Sequence">
    /// <![CDATA[
    /// LeagueLocations leagues = LeagueLocations.ConstructLeagueLocations();
    /// List<LeagueSchedule> schedules = new(); // Now create the sequence of LeagueSchedule objects
    /// foreach (KeyValuePair<string, string> kvp in leagues.Locations)
    /// {
    ///     LeagueSchedule schedule = LeagueSchedule.ConstructLeagueSchedule(kvp.Value);
    ///     schedules.Add(schedule);
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public sealed class LeaguesData
    {
        /// <summary>
        /// Creates an "empty" instance of this class, that is, the <paramref cref="LeagueSchedules"/> is the empty
        /// sequence.
        /// </summary>
        /// <remarks>
        /// Because this constructor is private, generally the only way to create an instance is using the static
        /// <see cref="ConstructLeaguesData(string?, Action{string})"/> method or via deserialization. 
        /// That's in fact what a builder application does. It only constructs the data store once and then updates the 
        /// schedule games as the games are completed.
        /// </remarks>
        private LeaguesData()
        {
            BuildDate = DateTime.Now;
            LeagueSchedules = Enumerable.Empty<LeagueSchedule>();
        }

        /// <summary>
        /// Gets and sets the time stamp when the instance is constructed or updated, typically when the 
        /// <see cref="LeagueSchedules"/> sequence is constructed or the elements updated.
        /// </summary>
        public DateTime BuildDate
        {
            get;
            set;
        }

        /// <summary>
        /// Get and initializes the sequences of game schedules for each league. 
        /// </summary>
        /// <remarks>
        /// Use the  <see cref="LeagueSchedule.ConstructLeagueSchedule(string)"/> method to build the sequence. The sequence
        /// can be altered only during initialization, but the elements of the sequence can be (and are) modified when additional
        /// game data are available
        /// </remarks>
        public IEnumerable<LeagueSchedule> LeagueSchedules
        {
            get;
            init;
        }

        /// <summary>
        /// Constructs an instance of this class which is the data store for all information about the SaddleBrooke senior
        /// softball games.
        /// </summary>
        /// <param name="saddleBrookeSeniorSoftball">The optional value that specifies the URL where the league schedules
        /// and scheduled games can be found. If not specified, <c>https://saddlebrookesoftball.com/</c> is used.
        /// </param>
        /// <param name="message">
        /// A delegate that can be used by the calling code to receive notifications during the construction process. 
        /// </param>
        /// <returns>A <c>LeaguesData</c> instance; <c>null</c> is never returned.</returns>
        /// <exception cref="InvalidOperationException">If any error occurs that prevents the data store to be built. The
        /// most common error occurs if code that extracts the information from the Web site pages is not valid. 
        /// </exception>
        public static LeaguesData ConstructLeaguesData(string? saddleBrookeSeniorSoftball = null, Action<string>? message = null)
        {
            LeaguesData leaguesData;

            Action<string> callback;
            if (message == null)
            {
                callback = (m) => Console.WriteLine(m);
            }
            else
            {
                callback = message;
            }

            try
            {
                callback("Beginning construction of League Locations");

                LeagueLocations leagues = LeagueLocations.ConstructLeagueLocations(saddleBrookeSeniorSoftball);
                callback($"Constructed LeagueLocations object. There are {leagues.Locations.Count} leagues.");

                List<LeagueSchedule> schedules = [];
                foreach (KeyValuePair<string, string> kvp in leagues.Locations)
                {
                    LeagueSchedule schedule = LeagueSchedule.ConstructLeagueSchedule(kvp.Value);
                    if (!schedule.IsEmpty)
                    {
                        schedules.Add(schedule);
                        callback($"Created schedule for {schedule.LeagueDescription}");
                    }
                    else
                    {
                        callback($"No scheduled games found for {kvp.Key}");
                    }
                }

                leaguesData = new()
                {
                    BuildDate = DateTime.Now,
                    LeagueSchedules = schedules.ToList()
                };

                callback($"Leagues data store created at {leaguesData.BuildDate:dddd MMMM d, yyyy a\\t hh:mm:ss tt}");
                callback("END");
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Unable to construct the data store.", exception);
            }

            return leaguesData;
        }

        /// <summary>
        /// Constructs and "empty" <see cref="LeaguesData"/> object, that is, one whose <see cref="BuildDate"/> property
        /// is <c>DateTime.MinValue</c> and whose <see cref="LeagueSchedules"/> property is the empty sequence.
        /// </summary>
        public static LeaguesData Empty
        {
            get
            {
                LeaguesData leaguesData = new()
                {
                    BuildDate = DateTime.MinValue
                };

                return leaguesData;
            }
        }

        /// <summary>
        /// Overrides the default <see cref="object.ToString()"/> method to provide summary descriptions and value of the
        /// two properties of this class.
        /// properties.
        /// </summary>
        /// <remarks>
        /// This method is most useful when writing information to the logging system.
        /// </remarks>
        /// <returns>
        /// A string that represents the current instance. For example,
        /// <code language="cs">
        /// Build Date 12/11/2023 11:19:04 AM; Number of Leagues 9
        /// </code>
        /// </returns>
        public override string ToString()
        {
            return $"Build Date {BuildDate}; Number of Leagues {LeagueSchedules.Count()}";
        }
    }
}
