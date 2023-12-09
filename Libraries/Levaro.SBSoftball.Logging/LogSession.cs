namespace Levaro.SBSoftball.Logging
{
    /// <summary>
    /// This class is just a container for log entries (<see cref="LogEntry"/> objects) that share the same session ID>
    /// </summary>
    public class LogSession
    {
        /// <summary>
        /// Creates a new <see cref="LogSession"/> instance with default values for the two properties.
        /// </summary>
        public LogSession()
        {
            BuildDate = DateTime.Now;
            Session = Guid.Empty;
            LogEntries = Enumerable.Empty<LogEntry>();
        }

        /// <summary>
        /// Gets and initializes the time stamp of the first log entry in the logging session.
        /// </summary>
        /// <remarks>
        /// This is convenient in order the sessions descending by build date, but the entries in each session in ascending
        /// order by the entry creation time.
        /// </remarks>
        public DateTime BuildDate
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the session ID (<see cref="Guid"/>) property.
        /// </summary>
        public Guid Session
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the sequence of <see cref="LogEntry"/> objects have the <see cref="Session"/> property value.
        /// </summary>
        public IEnumerable<LogEntry> LogEntries
        {
            get;
            init;
        }

    }
}
