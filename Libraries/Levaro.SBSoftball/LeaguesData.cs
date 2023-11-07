namespace Levaro.SBSoftball
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
    /// <code language="c#" title="Sample Code to Populated the Data Store">
    /// <![CDATA[
    /// LeaguesData leaguesData = new();  // The build date is the current date and time, but the 
    ///                                   // LeagueSchedules is the empty sequence.
    /// LeagueLocations leagues = LeagueLocations.ConstructLeagueLocations();
    /// List<LeagueSchedule> schedules = new(); // Now create the sequence of LeagueSchedule objects
    /// foreach (KeyValuePair<string, string> kvp in leagues.Locations)
    /// {
    ///     LeagueSchedule schedule = LeagueSchedule.ConstructLeagueSchedule(kvp.Value);
    ///     schedules.Add(schedule);
    ///     Console.WriteLine($"Created schedule for {schedule.LeagueDescription}");
    /// }
    /// ]]>
    /// </code>
    /// </para>
    /// </remarks>
    public class LeaguesData
    {
        /// <summary>
        /// Creates an "empty" instance of this class, that is, the <paramref cref="LeagueSchedules"/> is the empty
        /// sequence.
        /// </summary>
        public LeaguesData()
        {
            BuildDate = DateTime.Now;
            LeagueSchedules = Enumerable.Empty<LeagueSchedule>();
        }

        /// <summary>
        /// Gets and sets the time stamp when the instance was constructed, typically when the <see cref="LeagueSchedules"/>
        /// sequence is constructed.
        /// </summary>
        public DateTime BuildDate
        {
            get;
            set;
        }

        /// <summary>
        /// Get and sets the sequences of games schedules for each league. Use the 
        /// <see cref="LeagueSchedule.ConstructLeagueSchedule(string)"/> method to build the sequence.
        /// </summary>
        public IEnumerable<LeagueSchedule> LeagueSchedules
        {
            get;
            set;
        }
    }
}
