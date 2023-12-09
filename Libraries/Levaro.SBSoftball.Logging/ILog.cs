using System.Runtime.CompilerServices;

namespace Levaro.SBSoftball.Logging
{
    /// <summary>
    /// Interface specification for logging.
    /// </summary>
    /// <remarks>
    /// The class <see cref="Log"/> implements this interface.
    /// </remarks>
    public interface ILog : IDisposable
    {
        /// <summary>
        /// Gets and initializes the fully qualified path to the file to which the messages are written.
        /// </summary>
        /// <remarks>
        /// Implementations can only set this property when the instance is created.
        /// </remarks>
        string LogFilePath { get; init; }

        /// <summary>
        /// Gets the unique session ID.
        /// </summary>
        Guid Session { get; }

        /// <summary>
        /// This methods releases all resources, for example, streams and files used by the implementation.
        /// </summary>
        void Close();

        /// <summary>
        /// Part of the <see cref="IDisposable"/> implementation
        /// </summary>
        new void Dispose();


        /// <summary>
        /// Starts a new logging session.
        /// </summary>
        /// <remarks>
        /// Messages that are in the same session must have the same session ID.
        /// </remarks>
        void Start();

        /// <summary>
        /// Ends the current logging session.
        /// </summary>
        void Stop();

        /// <summary>
        /// Specifies a <see cref="LogEntry"/> to write to the log file.
        /// </summary>
        /// <param name="entry">An <c>LogEntry</c>instance</param>
        /// <remarks>
        /// Log messages are <c>LogEntry</c> objects -- the various <c>WriteLine</c> classes make common scenarios easier,
        /// but the constructed entries are implementation dependent.
        /// </remarks>
        void WriteLine(LogEntry entry);

        /// <summary>
        /// A method that can build a <see cref="LogEntry"/> instance and use it to write to the log file.
        /// </summary>
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
        /// The name of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerMemberNameAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member name from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>.
        /// </param>
        /// <param name="callerFilePath">
        /// The name of the file path of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerFilePathAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        /// <param name="callerLineNumber">
        /// The name of the line number of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerLineNumberAttribute"/> on the parameter with a default value of 0.
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        void WriteLine(DateTime dateTime, LogCategory category, string text, object? instance,
                       string callerMemberName = "",
                       string callerFilePath = "",
                       int callerLineNumber = 0);

        /// <summary>
        /// Write text and an object instance to the log file.
        /// </summary>
        /// <remarks>
        /// It is up to implementations to set the remaining properties of the a <see cref="LogEntry"/> object.
        /// </remarks>
        /// <param name="text">A text string</param>
        /// <param name="instance">An object instance</param>
        /// <param name="callerMemberName">
        /// The name of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerMemberNameAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member name from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>.
        /// </param>
        /// <param name="callerFilePath">
        /// The name of the file path of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerFilePathAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        /// <param name="callerLineNumber">
        /// The name of the line number of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerLineNumberAttribute"/> on the parameter with a default value of 0.
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        void WriteLine(string text, object instance,
                       string callerMemberName = "",
                       string callerFilePath = "",
                       int callerLineNumber = 0);
        /// <summary>
        /// Write a <see cref="LogCategory"/> enumeration value, text and an object to the log file.
        /// </summary>
        /// <remarks>
        /// It is up to implementations to set the remaining properties of the a <see cref="LogEntry"/> object.
        /// </remarks>
        /// <param name="category">A <c>LogCategory</c> value</param>
        /// <param name="text">A text string</param>
        /// <param name="instance">An object instance</param>
        /// <param name="callerMemberName">
        /// The name of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerMemberNameAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member name from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>.
        /// </param>
        /// <param name="callerFilePath">
        /// The name of the file path of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerFilePathAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        /// <param name="callerLineNumber">
        /// The name of the line number of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerLineNumberAttribute"/> on the parameter with a default value of 0.
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        void WriteLine(LogCategory category, string text, object instance,
                       string callerMemberName = "",
                       string callerFilePath = "",
                       int callerLineNumber = 0);
        /// <summary>
        /// Write a <see cref="LogCategory"/> enumeration value and text to the log file.
        /// </summary>
        /// <remarks>
        /// It is up to implementations to set the remaining properties of the a <see cref="LogEntry"/> object.
        /// </remarks>
        /// <param name="category">A <c>LogCategory</c> value</param>
        /// <param name="text">A text string</param>
        /// <param name="callerMemberName">
        /// The name of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerMemberNameAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member name from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>.
        /// </param>
        /// <param name="callerFilePath">
        /// The name of the file path of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerFilePathAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        /// <param name="callerLineNumber">
        /// The name of the line number of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerLineNumberAttribute"/> on the parameter with a default value of 0.
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        void WriteLine(LogCategory category, string text,
                       string callerMemberName = "",
                       string callerFilePath = "",
                       int callerLineNumber = 0);

        /// <summary>
        /// Write text to the log file.
        /// </summary>
        /// <remarks>
        /// It is up to implementations to set the remaining properties of the a <see cref="LogEntry"/> object.
        /// </remarks>
        /// <param name="text">A text string</param>
        /// <param name="callerMemberName">
        /// The name of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerMemberNameAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member name from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>.
        /// </param>
        /// <param name="callerFilePath">
        /// The name of the file path of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerFilePathAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        /// <param name="callerLineNumber">
        /// The name of the line number of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerLineNumberAttribute"/> on the parameter with a default value of 0.
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        void WriteLine(string text,
                       string callerMemberName = "",
                       string callerFilePath = "",
                       int callerLineNumber = 0);


        /// <summary>
        /// Write text and an <see cref="Exception"/>instance to the log file.
        /// </summary>
        /// <remarks>
        /// It is up to implementations to set the remaining properties of the a <see cref="LogEntry"/> object.
        /// </remarks>
        /// <param name="text">A text string</param>
        /// <param name="exception">An instance of an <see cref="Exception"/></param>
        /// <param name="callerMemberName">
        /// The name of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerMemberNameAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member name from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>.
        /// </param>
        /// <param name="callerFilePath">
        /// The name of the file path of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerFilePathAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        /// <param name="callerLineNumber">
        /// The name of the line number of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerLineNumberAttribute"/> on the parameter with a default value of 0.
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        void WriteLine(string text, Exception exception,
                       string callerMemberName = "",
                       string callerFilePath = "",
                       int callerLineNumber = 0);


        /// <summary>
        /// Write text contain the contents of an <see cref="Exception"/>
        /// </summary>
        /// <remarks>
        /// It is up to implementations to set the remaining properties of the a <see cref="LogEntry"/> object and how
        /// the exception is rendered to text.
        /// </remarks>
        /// <param name="exception">An instance of an <c>Exception</c></param>
        /// <param name="callerMemberName">
        /// The name of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerMemberNameAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member name from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>.
        /// </param>
        /// <param name="callerFilePath">
        /// The name of the file path of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerFilePathAttribute"/> on the parameter with a default value of the empty string. 
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param>
        /// <param name="callerLineNumber">
        /// The name of the line number of the member calling this method. Although not required, an implementation should use
        /// the <see cref="CallerLineNumberAttribute"/> on the parameter with a default value of 0.
        /// In that case the actual member file path from where this method is called is returned from the 
        /// <see cref="System.Runtime.CompilerServices"/>
        /// </param> 
        void WriteLine(Exception exception,
                       string callerMemberName = "",
                       string callerFilePath = "",
                       int callerLineNumber = 0);
    }
}