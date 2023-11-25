using System.Runtime.CompilerServices;

namespace Levaro.SBSoftball.Logging
{
    public interface ILog : IDisposable
    {
        bool AutoFlush { get; set; }

        string LogFilePath { get; set; }

        Guid Session { get; }

        bool Started { get; set; }

        bool UseMemoryStream { get; set; }

        StreamWriter? Writer { get; set; }

        void Close();

        new void Dispose();

        void Start();

        void Stop();

        void WriteLine(DateTime dateTime, LogCategory category, string text, object instance,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerFilePath] string callerFilePath = "",
                      [CallerLineNumber] int callerLineNumber = 0);

        void WriteLine(Exception exception,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerFilePath] string callerFilePath = "",
                      [CallerLineNumber] int callerLineNumber = 0);

        void WriteLine(LogCategory category, string text,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerFilePath] string callerFilePath = "",
                      [CallerLineNumber] int callerLineNumber = 0);

        void WriteLine(LogCategory category, string text, object instance,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerFilePath] string callerFilePath = "",
                      [CallerLineNumber] int callerLineNumber = 0);

        void WriteLine(LogEntry entry);

        void WriteLine(string text,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerFilePath] string callerFilePath = "",
                      [CallerLineNumber] int callerLineNumber = 0);

        void WriteLine(string text, Exception exception,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerFilePath] string callerFilePath = "",
                      [CallerLineNumber] int callerLineNumber = 0);

        void WriteLine(string text, object instance,
                      [CallerMemberName] string callerMemberName = "",
                      [CallerFilePath] string callerFilePath = "",
                      [CallerLineNumber] int callerLineNumber = 0);

    }
}