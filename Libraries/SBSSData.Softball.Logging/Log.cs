using System.Runtime.CompilerServices;

using Newtonsoft.Json;

using SBSSData.Softball.Common;

namespace SBSSData.Softball.Logging
{
    /// <summary>
    /// Provides services to write messages to a log file or stream.
    /// </summary>
    /// <remarks>
    /// A <c>Log</c> instance uses an existing log file or creates a new for logging messages. The class provides a collection of 
    /// <see cref="WriteLine(LogEntry)"/> methods to allow messages to be posted to the backing store. The backing store is
    /// a text file whose entries are JSON objects.
    /// <para>
    /// To use the logging facility, follow these steps:
    /// <list type="number">
    /// <item>
    ///     <description>
    ///         Create a <see cref="Log"/> instance using the constructor <see cref="Log(string, bool)"/> specifying a full path
    ///         for the log file. If this is a new file, it will be created; if it exists, the new messages are appended.
    ///     </description>
    /// </item>
    /// <item>
    ///     <description>
    ///         Call the <see cref="Log.Start()"/> method. It creates a unique identifier for the current logging session.
    ///     </description>
    ///</item>
    /// <item>
    ///     <description>
    ///         Use one of the eight variations of the <see cref="WriteLine(LogEntry)"/> to write a message to the log file. The
    ///         <c>WriteLine</c> methods allow you set a category (severity), time stamp, session either explicitly or implicitly.
    ///     </description>
    ///</item>
    /// <item>
    ///     <description>
    ///         To end the session, call the <see cref="Log.Stop()"/> method.
    ///     </description>
    ///</item>
    ///<item>
    ///     <description>
    ///         You can continue logging but with a different session by going back to step (2) again.
    ///     </description>
    ///</item>
    ///<item>
    ///     <description>
    ///         When done, call the <see cref="Log.Close()"/> to release any resources.
    ///     </description>
    ///</item>
    ///<item>
    ///     <description>
    ///         Finally, you can use the static <see cref="ReadLog(string)"/> to format and return the log file as a sequence
    ///         of <see cref="LogSession"/> objects.
    ///     </description>
    ///</item>
    /// </list>
    /// </para>
    /// </remarks>
    public sealed class Log : IDisposable
    {
        /// <summary>
        /// This private constructor creates an "empty" instance to make sure that all properties have default values.
        /// </summary>
        private Log()
        {
            LogFilePath = string.Empty;
            Writer = null;
            IsClosed = false;
        }

        /// <summary>
        /// Constructs a <see cref="Log"/> instance where messages are written to a text file whose location is specified by
        /// the <paramref name="logFilePath"/> parameter. 
        /// </summary>
        /// <param name="logFilePath">
        /// The full file path where the log messages are written. If the parameter is <c>null</c> or empty an exception is
        /// thrown. If the file does not exist it will be created; otherwise any messages will be appended to the existing
        /// file when the <see cref="Stop()"/> method is called.
        /// </param>
        /// <param name="consoleOutput">If <c>true</c>, all messages are also written to the console as well as to the logging
        /// file. This is the default; otherwise messages are not displayed to the console. The information displayed is 
        /// a string representation of  <see cref="LogEntry"/>, the object which is used when writing to a file. The
        /// JSON representation of the <c>LogEntry</c> object is written to the file.</param>
        /// <seealso cref="LogEntry.ToString()"/>
        /// <exception cref="InvalidOperationException">if the <paramref name="logFilePath"/> is either 
        /// <c>null</c>or empty.</exception>
        public Log(string logFilePath, bool consoleOutput = true)
        {
            LogFilePath = CheckForLogFile(logFilePath);
            ConsoleOutput = consoleOutput;

            // This will append to a file that already exists and has the right format, or overwrite the existing file. If the
            // file doesn't exist, the it will be created.
            Writer = new StreamWriter(LogFilePath, true);
            Start();
        }

        /// <summary>
        /// A private help method used by the <see cref="Log.Log(string, bool)"/> constructor to ensure that the log file path
        /// valid and if the file exists is a <c>Log</c> file.
        /// </summary>
        /// <param name="logFilePath">The fully qualified path to a log file that has a .log extension.</param>
        /// <returns>
        /// The value of the <paramref name="logFilePath"/> if the file is either created or if is the path of
        /// an existing <see cref="Log"/> file.
        /// </returns>
        /// <exception cref="InvalidOperationException">If the <paramref name="logFilePath"/> is not correctly formatted or
        /// references an existing file that does not appear to be a log file.</exception>
        private static string CheckForLogFile(string logFilePath)
        {
            if (string.IsNullOrEmpty(logFilePath) ||
                      !Path.IsPathFullyQualified(logFilePath) ||
                      !Path.GetExtension(logFilePath).Equals(".log", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A fully qualified file path with a \".log\" extension must be specified");
            }

            if (File.Exists(logFilePath))
            {
                try
                {
                    string text = File.ReadAllText(logFilePath);
                    if (string.IsNullOrEmpty(text) || (text.First() != '{') || !text.EndsWith("},\r\n"))
                    {
                        // This is not a log file, so create a new empty file
                        File.Create(logFilePath).Close();
                    }
                }
                catch (IOException exception)
                {
                    throw new InvalidOperationException("Error checking for the correct log format", exception);
                }
            }
            else
            {
                string? folder = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }

            return logFilePath;
        }

        /// <summary>
        /// Creates a new logging session by setting the <see cref="Session"/> property to a unique identifier 
        /// (<see cref="Guid"/>).
        /// </summary>
        public void Start()
        {
            Session = Guid.NewGuid();
        }

        /// <summary>
        /// End a logging session by setting the <see cref="Session"/> property to <see cref="Guid.Empty"/>.
        /// </summary>
        public void Stop()
        {
            Session = Guid.Empty;
        }

        /// <summary>
        /// Gets and sets the fully qualified path to the log file.
        /// </summary>
        public string LogFilePath
        {
            get;
            init;
        }

        /// <summary>
        /// This private property is initialized by the constructors and is used to write <see cref="LogEntry"/> messages to
        /// log from the <see cref="WriteLine(LogEntry)"/> method.
        /// </summary>
        private StreamWriter? Writer
        {
            get;
            init;
        }

        /// <summary>
        /// Get and sets the value that determines whether the log message is displayed on the console as well as written
        /// to the log file.
        /// </summary>
        /// <remarks>
        /// By default this is set to true in the <see cref="Log.Log(string, bool)"/> constructor but can be overridden by
        /// providing a value to for the <c>consoleOutput</c> parameter in the constructor call. It also can be toggled at any time
        /// during a logging session by changing the value of the property.
        /// </remarks>
        public bool ConsoleOutput
        {
            get;
            set;
        }

        /// <summary>
        /// Gets and sets the logging session unique ID. The value is only set by methods (in particular, the <see cref="Start()"/>
        /// and <see cref="Stop()"/> methods. The session be explicitly changed otherwise.
        /// </summary>
        public Guid Session
        {
            get;
            private set;
        }

        /// <summary>
        /// Writes the serialized JSON string for the <see cref="LogEntry"/> instance to the log file.
        /// </summary>
        /// <remarks>
        /// This is the only of the <c>WriteLine</c> methods that actually writes to the log file. After writing to the
        /// log file, the <see cref="LogEntry.ToString()"/> method results are written to the console if the 
        /// see cref="ConsoleOutput"/> property is <c>true.</c>
        /// <para>
        /// Typically this method should not be called directly, but rather use one of the other <c>WriteLine</c> which checks
        /// the <see cref="Session"/> property value and takes action accordingly, and sets the <see cref="LogEntry.Date"/> to
        /// the current time.
        /// </para>
        /// </remarks>
        /// <param name="entry">The <c>LogEntry</c> instance written to the log file.</param>
        /// <see cref="Log.WriteLine(DateTime, LogCategory, string, object?, string, string, int)"/>
        public void WriteLine(LogEntry entry)
        {
            Writer?.WriteLine($"{entry.ToJsonString()},");
            if (ConsoleOutput)
            {
                Console.WriteLine(entry.ToString());
            }
        }

        /// <summary>
        /// Like all the <c>WriteLine</c> methods except for <see cref="Log.WriteLine(LogEntry)"/>, this method
        /// calls that method after building a <see cref="LogEntry"/> instance using the parameters passed to it.
        /// </summary>
        /// <code>
        /// LogEntry entry = new(Session,
        ///                      dateTime,
        ///                      category,
        ///                      text,
        ///                      instance,
        ///                      callerMemberName,
        ///                      Path.GetFileName(callerFilePath),
        ///                      callerLineNumber);
        /// WriteLine(entry);
        /// </code>
        /// <param name="dateTime">The time stamp of the message.</param>
        /// <param name="category">
        /// The <see cref="LogCategory"/> of the message. If the <see cref="LogCategory.StartSession"/> is specified, the 
        /// <see cref="Session"/> property is assigned a new ID (<c>Guid.NewGuid()</c>> before the <c>LogEntry</c> class
        /// is constructed. If the <see cref="LogCategory.EndSession"/>, <c>Session</c> property is set to 
        /// <see cref="Guid.Empty"/> to indicate the session is empty.
        /// new ID 
        /// </param>
        /// <param name="text">The text of message.</param>
        /// <param name="instance">An object may provide additional information. For errors, this could be an 
        /// <see cref="Exception"/> for example.</param>
        /// <param name="callerMemberName">
        /// This parameter is decorated with the <see cref="CallerMemberNameAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual member name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerFilePath">
        /// This parameter is decorated with the <see cref="CallerFilePathAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual file path of the member from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerLineNumber">
        /// This parameter is decorated with the <see cref="CallerLineNumberAttribute"/> and is
        /// optional, the default value is 0 (<c>int</c>). When not specified, the actual line  name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// If there is no active session and the <see cref="LogCategory"/> is not <see cref="LogCategory.StartSession"/>.
        /// </exception>
        public void WriteLine(DateTime dateTime, LogCategory category, string text, object? instance,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {

            if (category == LogCategory.StartSession)
            {
                Session = Guid.NewGuid();
            }
            else if (Session == Guid.Empty)
            {
                throw new InvalidOperationException("No session ID is available. The log entry category must be \"StartSession\"");
            }

            LogEntry entry = new(Session,
                                 dateTime,
                                 category,
                                 text,
                                 instance,
                                 callerMemberName,
                                 Path.GetFileName(callerFilePath),
                                 callerLineNumber);
            WriteLine(entry);
            if (category == LogCategory.EndSession)
            {
                Session = Guid.Empty;
            }
        }

        /// <summary>
        /// Specifies text and an object instance to write to the log file.
        /// </summary>
        /// <remarks>
        /// This method just invokes <see cref="WriteLine(LogEntry)"/>  after building a <see cref="LogEntry"/> instance 
        /// using the parameters passed to it. 
        /// <code language="c#">
        /// LogEntry entry = new(Session,
        ///                      DateTime.Now,
        ///                      LogCategory.Info,
        ///                      text,
        ///                      instance,
        ///                      callerMemberName,
        ///                      Path.GetFileName(callerFilePath),
        ///                      callerLineNumber); ;
        /// WriteLine(entry);
        /// </code>
        /// </remarks>
        /// <param name="text">The message text.</param>
        /// <param name="instance">An instance of an object. Although <c>null</c> is accepted, in that case a different
        /// method variant should be used.</param>
        /// <param name="callerMemberName">
        /// This parameter is decorated with the <see cref="CallerMemberNameAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual member name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerFilePath">
        /// This parameter is decorated with the <see cref="CallerFilePathAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual file path of the member from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerLineNumber">
        /// This parameter is decorated with the <see cref="CallerLineNumberAttribute"/> and is
        /// optional, the default value is 0 (<c>int</c>). When not specified, the actual line  name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        public void WriteLine(string text, object instance,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {
            LogEntry entry = new(Session,
                                 DateTime.Now,
                                 LogCategory.Info,
                                 text,
                                 instance,
                                 callerMemberName,
                                 Path.GetFileName(callerFilePath),
                                 callerLineNumber); ;
            WriteLine(entry);
        }

        /// <summary>
        /// Specifies a <see cref="LogCategory"/>, text and an object to write to the log file.
        /// </summary>
        /// <remarks>
        /// This method just invokes <see cref="WriteLine(LogEntry)"/>  after building a <see cref="LogEntry"/> instance 
        /// using the parameters passed to it. 
        /// <code language="c#">
        /// LogEntry entry = new(Session,
        ///                      DateTime.Now,
        ///                      category,
        ///                      text,
        ///                      instance,
        ///                      callerMemberName,
        ///                      Path.GetFileName(callerFilePath),
        ///                      callerLineNumber); ;
        /// WriteLine(entry);
        /// </code>
        /// </remarks>
        /// <param name="category">The <see cref="LogCategory"/> enumeration value.</param>
        /// <param name="text">The message text.</param>
        /// <param name="instance">An instance of an object. Although <c>null</c> is accepted, in that case a different
        /// method variant should be used.</param>
        /// <param name="callerMemberName">
        /// This parameter is decorated with the <see cref="CallerMemberNameAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual member name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerFilePath">
        /// This parameter is decorated with the <see cref="CallerFilePathAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual file path of the member from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerLineNumber">
        /// This parameter is decorated with the <see cref="CallerLineNumberAttribute"/> and is
        /// optional, the default value is 0 (<c>int</c>). When not specified, the actual line  name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        public void WriteLine(LogCategory category, string text, object instance,
                             [CallerMemberName] string callerMemberName = "",
                             [CallerFilePath] string callerFilePath = "",
                             [CallerLineNumber] int callerLineNumber = 0)

        {
            LogEntry entry = new(Session,
                                 DateTime.Now,
                                 category,
                                 text,
                                 instance,
                                 callerMemberName,
                                 Path.GetFileName(callerFilePath),
                                 callerLineNumber); ;
            WriteLine(entry);

        }

        /// <summary>
        /// Specifies a <see cref="LogCategory"/> and text to write to the log file.
        /// </summary>
        /// <remarks>
        /// This method just invokes <see cref="WriteLine(LogEntry)"/>  after building a <see cref="LogEntry"/> instance 
        /// using the parameters passed to it. 
        /// <code language="c#">
        /// LogEntry entry = new(Session,
        ///                      DateTime.Now,
        ///                      category,
        ///                      text,
        ///                      null,
        ///                      callerMemberName,
        ///                      Path.GetFileName(callerFilePath),
        ///                      callerLineNumber); ;
        /// WriteLine(entry);
        /// </code>
        /// </remarks>
        /// <param name="category">The <see cref="LogCategory"/> enumeration value.</param>
        /// <param name="text">The message text.</param>
        /// <param name="callerMemberName">
        /// This parameter is decorated with the <see cref="CallerMemberNameAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual member name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerFilePath">
        /// This parameter is decorated with the <see cref="CallerFilePathAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual file path of the member from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerLineNumber">
        /// This parameter is decorated with the <see cref="CallerLineNumberAttribute"/> and is
        /// optional, the default value is 0 (<c>int</c>). When not specified, the actual line  name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        public void WriteLine(LogCategory category, string text,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)

        {
            LogEntry entry = new(Session,
                                 DateTime.Now,
                                 category,
                                 text,
                                 null,
                                 callerMemberName,
                                 Path.GetFileName(callerFilePath),
                                 callerLineNumber); ;
            WriteLine(entry);
        }

        /// <summary>
        /// Specifies text to write to the log file.
        /// </summary>
        /// <remarks>
        /// This method just invokes <see cref="WriteLine(LogEntry)"/>  after building a <see cref="LogEntry"/> instance 
        /// using the parameters passed to it. 
        /// <code language="c#">
        /// LogEntry entry = new(Session,
        ///                      DateTime.Now,
        ///                      LogCategory.Info,
        ///                      text,
        ///                      null,
        ///                      callerMemberName,
        ///                      Path.GetFileName(callerFilePath),
        ///                      callerLineNumber);
        /// WriteLine(entry);                     
        /// </code>
        /// </remarks>
        /// <param name="text">The message text.</param>
        /// <param name="callerMemberName">
        /// This parameter is decorated with the <see cref="CallerMemberNameAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual member name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerFilePath">
        /// This parameter is decorated with the <see cref="CallerFilePathAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual file path of the member from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerLineNumber">
        /// This parameter is decorated with the <see cref="CallerLineNumberAttribute"/> and is
        /// optional, the default value is 0 (<c>int</c>). When not specified, the actual line  name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        public void WriteLine(string text,
                             [CallerMemberName] string callerMemberName = "",
                             [CallerFilePath] string callerFilePath = "",
                             [CallerLineNumber] int callerLineNumber = 0)

        {
            LogEntry entry = new(Session,
                                 DateTime.Now,
                                 LogCategory.Info,
                                 text,
                                 null,
                                 callerMemberName,
                                 Path.GetFileName(callerFilePath),
                                 callerLineNumber); ;
            WriteLine(entry);
        }


        /// <summary>
        /// Specifies text and an <see cref="Exception"/> instance to write to the log file.
        /// </summary>
        /// /// <remarks>
        /// This method just invokes <see cref="WriteLine(LogEntry)"/>  after building a <see cref="LogEntry"/> instance 
        /// using the parameters passed to it. 
        /// <code language="c#">
        /// LogEntry entry = new(Session,
        ///                      DateTime.Now,
        ///                      LogCategory.Error,
        ///                      text,
        ///                      exception,
        ///                      callerMemberName,
        ///                      Path.GetFileName(callerFilePath),
        ///                      callerLineNumber); 
        /// WriteLine(entry);
        /// </code>
        /// </remarks>
        /// <param name="text">The message text.</param>
        /// <param name="exception">An instance of an <see cref="Exception"/> class. Although <c>null</c> is accepted, in that case a different
        /// method variant should be used.</param>
        /// <param name="callerMemberName">
        /// This parameter is decorated with the <see cref="CallerMemberNameAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual member name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerFilePath">
        /// This parameter is decorated with the <see cref="CallerFilePathAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual file path of the member from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerLineNumber">
        /// This parameter is decorated with the <see cref="CallerLineNumberAttribute"/> and is
        /// optional, the default value is 0 (<c>int</c>). When not specified, the actual line  name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        public void WriteLine(string text, Exception exception,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)

        {
            LogEntry entry = new(Session,
                                 DateTime.Now,
                                 LogCategory.Error,
                                 text,
                                 exception,
                                 callerMemberName,
                                 Path.GetFileName(callerFilePath),
                                 callerLineNumber);
            WriteLine(entry);
        }

        /// <summary>
        /// Specifies an JSON representation of an <see cref="Exception"/> instance to write to the log file.
        /// </summary>
        /// <remarks>
        /// This method just invokes <see cref="WriteLine(LogEntry)"/>  after building a <see cref="LogEntry"/> instance 
        /// using the parameters passed to it. 
        /// <code language="c#">
        /// string text = exception != null ? exception.ToJsonString() : "Provided exception is null.";
        /// LogEntry entry = new(Session,
        ///                      DateTime.Now,
        ///                      LogCategory.Error,
        ///                      text,
        ///                      null,
        ///                      callerMemberName,
        ///                      Path.GetFileName(callerFilePath),
        ///                      callerLineNumber); ;
        /// WriteLine(entry);
        /// </code>
        /// </remarks>
        /// <param name="exception">
        /// An instance of an <see cref="Exception"/> class. Its JSON text representation is written to the log file.
        /// </param>
        /// <param name="callerMemberName">
        /// This parameter is decorated with the <see cref="CallerMemberNameAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual member name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerFilePath">
        /// This parameter is decorated with the <see cref="CallerFilePathAttribute"/> and is
        /// optional, the default value is the empty string. When not specified, the actual file path of the member from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        /// <param name="callerLineNumber">
        /// This parameter is decorated with the <see cref="CallerLineNumberAttribute"/> and is
        /// optional, the default value is 0 (<c>int</c>). When not specified, the actual line  name from where
        /// this method is called is returned from the <see cref="System.Runtime.CompilerServices"/>. Although you can set this
        /// value, it is best to let the system return the value for you.
        /// </param>
        public void WriteLine(Exception exception,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)

        {
            string text = exception != null ? exception.ToJsonString() : "Provided exception is null.";
            LogEntry entry = new(Session,
                                 DateTime.Now,
                                 LogCategory.Error,
                                 text,
                                 null,
                                 callerMemberName,
                                 Path.GetFileName(callerFilePath),
                                 callerLineNumber); ;
            WriteLine(entry);
        }

        /// <summary>
        /// Disposes the <see cref="Writer"/> <see cref="StreamWriter"/> object that is used to write to the log file.
        /// </summary>
        /// <remarks>
        /// Although you can use a <c>using</c> construct because the <see cref="IDisposable"/> interface is implemented, 
        /// that is typically awkward because many calls from different members of the application need access to the log file. 
        /// Before the application terminates, make sure that this method is called.
        /// </remarks>
        public void Close()
        {
            Dispose(true);
            IsClosed = true;
        }

        /// <summary>
        /// Static method to read and return the contents of a log file as a sequence of <see cref="LogSession"/> objects.
        /// </summary>
        /// <param name="logFilePath">The fully qualified path for the log file.</param>
        /// <returns>
        /// A sequence of <see cref="LogSession"/> objects. The empty sequence may be returned, but <c>null</c> never is.
        /// </returns>
        /// <exception cref="FileNotFoundException">If the file does not exist.</exception>
        /// <exception cref="InvalidOperationException">if the <paramref name="logFilePath"/> is not a fully qualified path
        /// with a .log extension or doesn't appear to be a log file built with the <see cref="Log"/> API.</exception>"
        public static IEnumerable<LogSession> ReadLog(string logFilePath)
        {
            IEnumerable<LogSession> logSessions = Enumerable.Empty<LogSession>();

            if (!File.Exists(logFilePath))
            {
                throw new FileNotFoundException($"{logFilePath} cannot be found.");
            }

            if (string.IsNullOrEmpty(logFilePath) ||
                      !Path.IsPathFullyQualified(logFilePath) ||
                      !Path.GetExtension(logFilePath).Equals(".log", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("A fully qualified file path with a \".log\" extension must be specified");
            }


            string text = File.ReadAllText(logFilePath);
            try
            {
                if (string.IsNullOrEmpty(text) || (text.First() != '{') || !text.EndsWith("},\r\n"))
                {
                    throw new InvalidOperationException("The file does not appear to be a log file.");
                }
            }
            catch (IOException exception)
            {
                throw new InvalidOperationException("Error checking for the correct log format", exception);
            }

            string logJson = $"[\r\n{text}\r\n]";
            IEnumerable<LogEntry> logEntryList = JsonConvert.DeserializeObject<List<LogEntry>>(logJson) ?? Enumerable.Empty<LogEntry>();
            foreach (IGrouping<Guid, LogEntry> group in logEntryList.OrderBy(e => e.Date).GroupBy(e => e.SessionId))
            {
                DateTime sessionTimeStamp = group.First().Date;
                logSessions = logSessions.Append(new LogSession()
                {
                    Session = group.Key,
                    LogEntries = group.ToList().OrderBy(e => e.Date),
                    BuildDate = sessionTimeStamp
                });
            };

            return logSessions.ToList().OrderByDescending(s => s.BuildDate);
        }

        /// <summary>
        /// Releases the <see cref="StreamWriter"/> resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        public bool IsClosed
        {
            get;
            set;
        }

        // IDisposable implementation
        private bool disposedValue = false; // To detect redundant calls
        /// <summary>
        /// Releases the StreamWriter by calling <see cref="StreamWriter.Dispose(bool)"/>
        /// </summary>
        /// <param name="disposing">Used to detect redundant calls. Necessary for
        /// implementation of the <see cref="IDisposable"/> interface.</param>
        public void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Writer?.Dispose();
                }

                disposedValue = true;
            }
        }
    }
}
