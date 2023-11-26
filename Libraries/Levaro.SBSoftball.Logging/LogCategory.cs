namespace Levaro.SBSoftball.Logging
{
    /// <summary>
    /// An enumeration of describing the use or purpose the the <see cref="LogEntry"/>.
    /// </summary>
    public enum LogCategory
    {
        /// <summary>
        /// The category is unknown.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The start of a logging session. All entries after a start of the same <see cref="LogEntry.SessionId"/>.
        /// </summary>
        StartSession = Unknown + 1,

        /// <summary>
        /// Tne ending of a logging session. The next <see cref="LogEntry"/> has a new session ID>
        /// </summary>
        EndSession = StartSession + 1,

        /// <summary>
        /// The log entry is for informational purposes.
        /// </summary>
        Info = EndSession + 1,

        /// <summary>
        /// This is a debugging entry just to help the debugging process. Typically only temporary.
        /// </summary>
        Debug = Info + 1,

        /// <summary>
        /// A warning, but does indicate failure.
        /// </summary>
        Warning = Debug + 1,

        /// <summary>
        /// An error has occurred, and often an exception or other problem that prevents continued execution.
        /// </summary>
        Error = Warning + 1
    }
}
