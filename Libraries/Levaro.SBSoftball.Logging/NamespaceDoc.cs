namespace Levaro.SBSoftball.Logging
{
    /// <summary>
    /// Provides interface and implementation for a file logging infrastructure.
    /// </summary>
    /// <remarks>
    /// The <see cref="LogEntry"/> class defines the elements that are persisted to the log file. The <see cref="Log"/>
    /// serializes the <c>LogEntry</c> object to JSON text and writes them to a file as requests are made to write a 
    /// message. One the <c>Log</c> is created, messages are grouped by session, which is determined by a unique 
    /// ID (<see cref="Guid"/>). During the lifetime of the <c>Log</c> instance, you can have as many sessions as needed.
    /// <para>
    /// The static method <see cref="Log.ReadLog(string)"/> reads a generated log file returns a sequence of 
    /// <see cref="LogSession"/> objects, each of which is basically a sequence of <c>LogEntry</c> objects sharing
    /// the same session ID.
    /// </para>
    /// </remarks>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class NamespaceDoc
    {
    }
}
