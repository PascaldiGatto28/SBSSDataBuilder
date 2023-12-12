using Levaro.SBSoftball.Common;
using Levaro.SBSoftball.Logging;

namespace Levaro.Application.SBSSDataStore
{
    internal class Program
    {
        internal static void Main()
        {
            Console.WriteLine($"\r\nSBSS Data Store Manager -- Building and updating the SBSS data store (Built on {DateTime.Now:dddd MMMM d, yyyy})\r\n");

            AppContext context = AppContext.Instance;
            try
            {
                using Log log = context.Log;
                log.WriteLine("Starting the Data store manager");
                try
                {
                    DataStoreManager.Run((context.Settings).Update);
                }
                catch (Exception exception)
                {
                    log.WriteLine("Error not caught", exception);
                    throw new InvalidOperationException("Exception encountered while the data store manage is executing", exception);
                }
                finally
                {
                    log.WriteLine("Completed.");
                    log.Stop();
                }
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine($"Unhandled exception during Log processing {exception}");
            }

            // Now process the log files. The .log file has been updated, so use Log.ReturnLog to get the a 
            // IEnumerable<LogSession> sequence for this execution. If there is already a serialization (.json), read that
            // and append the new sequence to it and then serialize it. It does mean we're reading and write some files, but
            // who cares?

            string logFilePath = context.Settings.LogFilePath;
            IEnumerable<LogSession> sessions = Log.ReadLog(logFilePath);
            string logSessionsFilePath = logFilePath.Replace(".log", ".json", StringComparison.Ordinal);
            IEnumerable<LogSession> loggedSessions = File.Exists(logSessionsFilePath) ?
                                                     logSessionsFilePath.Deserialize<IEnumerable<LogSession>>() :
                                                     Enumerable.Empty<LogSession>();

            sessions.ToList().AddRange(loggedSessions);
            sessions.Serialize(logSessionsFilePath);
        }
    }
}
