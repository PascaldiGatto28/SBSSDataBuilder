using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Levaro.SBSoftball.Logging
{
    /// <summary>
    /// Encapsulates the information that is output in the log file.
    /// </summary>
    /// <remarks>
    /// The <see cref="Log.WriteLine(LogEntry)"/> method, which all the other variants ultimately call, writes a JSON string
    /// representation of this class to the log file or stream.
    /// </remarks>
    public class LogEntry
    {
        /// <summary>
        /// Creates a new "empty" instance of the class by simply initializing all property to their default values.
        /// </summary>
        public LogEntry()
        {
            SessionId = Guid.Empty;
            Date = DateTime.Now;
            LogCategory = LogCategory.Unknown;
            LogText = string.Empty;
            ObjectInstance = null;
            CallerFileName = string.Empty;
            CallerMemberName = string.Empty;
            CallerLineNumber = -1;
        }

        /// <summary>
        /// Creates a new instance using the parameters to set all property values.
        /// </summary>
        /// <remarks>
        /// All properties can only be set using this constructor, and it is only called by <see 
        /// cref="Log.WriteLine(DateTime, LogCategory, string, object?, string, string, int)"/>. There is typically no reason
        /// to call the constructor directly.
        /// </remarks>
        /// <param name="sessionId">A globally unique identifier (see cref="Guid") that identifies a logging session.</param>
        /// <param name="dateTime">The time stamp of the start of the logging session.</param>
        /// <param name="category">A value of the <see cref="LogCategory"/> enumeration which describes the importance level
        /// of the log entry.</param>
        /// <param name="logText">Descriptive text of the log entry. If <c>null</c>, the empty string is used.</param>
        /// <param name="instance">An optional <see cref="object"/> value that provides more information about the
        /// the log entry. </param>
        /// <param name="callerMemberName">The optional member name from where the log entry was created. The default determines 
        /// the value automatically and is typically not set.</param>
        /// <param name="callerFileName">The optional class file name from where the log entry was created. The default 
        /// determines the value automatically and is typically not set.</param>
        /// <param name="callerLineNumber">The optional line number in the file from which the log entry was created. The default 
        /// determines the value automatically and is typically not set.</param>
        public LogEntry(Guid sessionId,
                        DateTime dateTime,
                        LogCategory category,
                        string logText,
                        object? instance,
                        string callerMemberName = "",
                        string callerFileName = "",
                        int callerLineNumber = -1)
        {
            SessionId = sessionId;
            Date = dateTime;
            LogCategory = category;
            LogText = logText ?? string.Empty;
            ObjectInstance = instance;
            CallerFileName = callerFileName;
            CallerMemberName = callerMemberName;
            CallerLineNumber = callerLineNumber;
        }

        /// <summary>
        /// Gets and initializes the session ID for the log entry.
        /// </summary>
        /// <remarks>
        /// A new value is created only when a session is started via the <see cref="Log.Start"/> method. After that, until
        /// <see cref="Log.Stop"/> is called, all the <see cref="Log.WriteLine(LogEntry)"/>a variants use the current session
        /// value which is stored in the <see cref="Log.Session"/> property.
        /// </remarks>
        public Guid SessionId
        {
            get;
            init;
        }

        /// <summary>
        /// Get and initializes the time stamp for the current <c>LogEntry</c>. It is typically set to <see cref="DateTime.Now"/>.
        /// </summary>
        public DateTime Date
        {
            get;
            init;
        }


        /// <summary>
        /// Gets and initializes the log entry category as on of the values of the <see cref="LogCategory"/> enumeration.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LogCategory LogCategory
        {
            get;
            init;
        }

        /// <summary>
        /// Get and initializes the log entry text. Typically this is always specified when creating a <see cref="LogEntry"/> to
        /// write the log file or stream.
        /// </summary>
        public string LogText
        {
            get;
            init;
        }

        /// <summary>
        /// An object to provide more information. It is most often used to display an exception or other object. Is is 
        /// optional and the value is <c>null</c>.
        /// </summary>
        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public object? ObjectInstance
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the value of name of the file from where this instance was created.
        /// This and the next two properties typically are never specified. The default values recover the appropriate data
        /// because each is decorated with an attribute that when the default is used, the actual value is recovered.
        /// </summary>
        /// <remarks>
        /// If you specify this value, you must decorate it with the <see cref="CallerFileName"/> attribute if you if you want the
        /// actual value to be returned. Typically you should not specify this parameter, but use the defaults.
        /// </remarks>
        public string CallerFileName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the value of name of the member from where this instance was created.
        /// The default values recover the appropriate data because each is decorated with an attribute that when the
        /// default is used.
        /// </summary>
        /// <remarks>
        /// If you specify this value, you must decorate it with the <see cref="CallerMemberName"/> attribute if you if you want 
        /// the actual value to be returned. Typically you should not specify this parameter, but use the defaults.
        /// </remarks>
        public string CallerMemberName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the value of line number in the file from where this instance was created.
        /// The default values recover the appropriate data because each is decorated with an attribute that when the
        /// default is used.
        /// </summary>
        /// <remarks>
        /// If you specify this value, you must decorate it with the <see cref="CallerLineNumber"/> attribute if you want the
        /// actual value to be returned. Typically you should not specify this parameter, but use the defaults.
        /// </remarks>
        public int CallerLineNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Get the "empty" <see cref="LogEntry"/>, that is one in which all properties are the default.
        /// </summary>
        public static LogEntry Empty
        {
            get
            {
                return new LogEntry();
            }
        }

        /// <summary>
        /// Overrides the default method and returns a string that represents this instance.
        /// </summary>
        /// <returns>
        /// A string having the <see cref="Date"/>, <see cref="LogCategory"/> and <see cref="LogText"/>, for example,
        /// "06:18:32 PM -- Info: Starting the Data store manager".
        /// </returns>
        public override string ToString()
        {
            return $"{Date:hh:mm:ss tt} -- {LogCategory}: {LogText}";
        }
    }
}
