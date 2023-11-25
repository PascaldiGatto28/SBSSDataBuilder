using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Levaro.SBSoftball.Logging
{
    public class LogEntry
    {
        public LogEntry()
        {
            Date = DateTime.Now;
            LogCategory = LogCategory.Unknown;
            LogText = string.Empty;
            ObjectInstance = null;
            CallerFileName = string.Empty;
            CallerMemberName = string.Empty;
            CallerLineNumber = -1;
        }

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
            //IsEmpty = false;
        }

        public Guid SessionId
        {
            get;
            set;
        }

        public DateTime Date
        {
            get;
            set;
        }


        [JsonConverter(typeof(StringEnumConverter))]
        public LogCategory LogCategory
        {
            get;
            set;
        }

        public string LogText
        {
            get;
            set;
        }

        [JsonProperty(TypeNameHandling = TypeNameHandling.Objects)]
        public object? ObjectInstance
        {
            get;
            set;
        }

        public string CallerFileName
        {
            get;
            set;
        }

        public string CallerMemberName
        {
            get;
            set;
        }

        public int CallerLineNumber
        {
            get;
            set;
        }

        public static LogEntry Empty
        {
            get
            {
                return new LogEntry();
            }
        }

        public override string ToString()
        {
            return $"{Date:hh:mm:ss tt} -- {LogCategory}: {LogText}";
        }
    }
}
