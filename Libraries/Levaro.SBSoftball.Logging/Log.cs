using System.Runtime.CompilerServices;

using Levaro.SBSoftball.Common;

using Newtonsoft.Json;

namespace Levaro.SBSoftball.Logging
{
    public sealed class Log : ILog, IDisposable
    {
        private Log()
        {
            LogFilePath = string.Empty;
            Writer = null;
        }

        public Log(StreamWriter writer, bool autoFlush = true)
        {
            Writer = writer;
            AutoFlush = autoFlush;
            Session = Guid.Empty;
            LogFilePath = string.Empty;
        }

        public Log(string logFilePath, bool autoFlush = true, bool useMemory = false, bool start = true, bool consoleOutput = true)
        {
            LogFilePath = logFilePath ?? string.Empty;
            AutoFlush = autoFlush;
            UseMemoryStream = useMemory;
            if (start)
            {
                Start();
            }
            ConsoleOutput = consoleOutput;

        }

        public string LogFilePath
        {
            get;
            set;
        }

        public bool UseMemoryStream
        {
            get;
            set;
        }

        public StreamWriter? Writer
        {
            get;
            set;
        }

        public bool AutoFlush
        {
            get;
            set;
        }

        public bool ConsoleOutput
        {
            get;
            set;
        }

        public Guid Session
        {
            get;
            private set;
        }

        public bool Started
        {
            get;
            set;
        }

        public void Start()
        {
            // Make sure the current log file (if present) is the new "json" format. The first character in the file
            // must a "{" character.
            if (File.Exists(LogFilePath))
            {
                try
                {
                    char firstChar = '{';
                    using (StreamReader streamReader = new(File.Open(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                    {
                        if (!streamReader.EndOfStream)
                        {
                            firstChar = Convert.ToChar(streamReader.Read());
                        }
                    }

                    if (firstChar != '{')
                    {
                        using FileStream stream = File.Create(LogFilePath);
                    }
                }
                catch (IOException exception)
                {
                    throw new InvalidOperationException("Error checking for correct log format", exception);
                }
            }

            if (UseMemoryStream)
            {
                Writer = new StreamWriter(new MemoryStream());
            }
            else
            {
                Writer = new StreamWriter(LogFilePath, true);
            }

            Writer.AutoFlush = AutoFlush;
            Session = Guid.NewGuid();
            Started = true;
        }

        public void Stop()
        {
            Writer?.Flush();
            if (Started)
            {
                Stream? baseStream = Writer?.BaseStream;
                if (baseStream is MemoryStream stream)
                {
                    byte[] buffer = stream.GetBuffer();
                    string logData = buffer.ByteArrayToString().Trim('\0');

                    // Now try to write the log file (maybe it's been closed by now), but if not, then create a new file and write
                    // to it.
                    try
                    {
                        File.AppendAllText(LogFilePath, logData);
                    }
                    catch (IOException)
                    {
                        string newLogPath = LogFilePath.AppendTimeStampToFileName();
                        File.WriteAllText(newLogPath, logData);
                    }
                }

                Started = false;
            }

            Writer?.Close();
            Session = Guid.Empty;
        }

        public bool IsWriterClosed() => ((Writer == null) || (Writer.BaseStream == null));

        public void WriteLine(LogEntry entry)
        {
            if (!IsWriterClosed())
            {
                Writer?.WriteLine($"{entry.ToJsonString()},");
            }
        }

        public void WriteLine(DateTime dateTime, LogCategory category, string text, object? instance,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {

            if (category == LogCategory.StartSession)
            {
                Session = Guid.NewGuid();
            }
            else if ((Session == Guid.Empty) && !IsWriterClosed())
            {
                throw new InvalidOperationException("No session ID is available. The log entry must be category \"StartSession\"");
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

            if (ConsoleOutput)
            {
                Console.WriteLine(entry.ToString());
            }
        }

        public void WriteLine(string text, object instance,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {
            WriteLine(DateTime.Now, LogCategory.Info, text, instance, callerMemberName, callerFilePath, callerLineNumber);
        }

        public void WriteLine(LogCategory category, string text, object instance,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {
            WriteLine(DateTime.Now, category, text, instance, callerMemberName, callerFilePath, callerLineNumber);
        }

        public void WriteLine(LogCategory category, string text,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {
            WriteLine(DateTime.Now, category, text, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        public void WriteLine(string text,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {
            WriteLine(DateTime.Now, LogCategory.Info, text, null, callerMemberName, callerFilePath, callerLineNumber);
        }

        public void WriteLine(string text, Exception exception,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {
            WriteLine(DateTime.Now, LogCategory.Error, text, exception, callerMemberName, callerFilePath, callerLineNumber);
        }

        public void WriteLine(Exception exception,
                              [CallerMemberName] string callerMemberName = "",
                              [CallerFilePath] string callerFilePath = "",
                              [CallerLineNumber] int callerLineNumber = 0)
        {
            string text = "Provided exception is null.";
            if (exception != null)
            {
                //string stack = string.IsNullOrEmpty(exception.StackTrace) ? "Stack Trace is empty" : exception.StackTrace;
                text = exception.ToJsonString();
            }

            WriteLine(LogCategory.Error, text, callerMemberName, callerFilePath, callerLineNumber);
        }

        public void Close()
        {
            Dispose(true);
        }

        public static IEnumerable<IEnumerable<LogEntry>> ReadLog(StreamReader streamReader)
        {
            string logJson = $"[\r\n{streamReader.ReadToEnd()}\r\n]";
            IEnumerable<LogEntry> logEntryList = JsonConvert.DeserializeObject<List<LogEntry>>(logJson) ?? Enumerable.Empty<LogEntry>();
            return logEntryList.OrderByDescending(e => e.Date).GroupBy(e => e.SessionId).Select(g => g.ToList());
        }

        public static IEnumerable<IEnumerable<LogEntry>> ReadLog(string logPath)
        {
            IEnumerable<IEnumerable<LogEntry>> logEntryList = Enumerable.Empty<IEnumerable<LogEntry>>();
            using (FileStream fileStream = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using StreamReader streamReader = new(fileStream);
                logEntryList = ReadLog(streamReader);
            }

            return logEntryList;
        }

        private bool disposedValue = false; // To detect redundant calls
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

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
