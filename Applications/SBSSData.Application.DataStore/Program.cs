using Newtonsoft.Json;

using SBSSData.Softball.Common;
using SBSSData.Softball.Logging;

namespace SBSSData.Application.DataStore
{
    /// <summary>
    /// The entry point for the application, setting up the environment for the actual work to create
    /// and maintain the data store.
    /// </summary>
    internal sealed class Program
    {
        /// <summary>
        /// The static method that is called to execute the console application. 
        /// </summary>
        /// <remarks>
        /// The  functions of the startup method are threefold:
        /// <list type="number">
        ///     <item>
        ///         An instance of the <see cref="AppContext"/> is created which provides services the
        ///         remainder of the code, in particular create a <see cref="Log"/> object that can be
        ///         used by any other classes or members.
        ///     </item>
        ///     <item>
        ///         Once the <c>AppContext</c> is created, the <see cref="DataStoreManager.Run"/> method
        ///         is invoked, passing an <see cref="AppSettings"/> property to it. The instance of the
        ///         <c>AppSettings</c> class is constructed by the <c>AppContext</c> when it is created.
        ///     </item>
        ///     <item>
        ///         When the <c>DataStoreManager</c> returns, and errors are caught and logged. The log file
        ///         is processed and serialized to a JSON file. Finally the Log is disposed to clean everything
        ///         up.
        ///     </item>
        /// </list>
        /// </remarks>
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
                    log.Close();
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
            IEnumerable<LogSession>? loggedSessions = File.Exists(logSessionsFilePath) ?
                                                     JsonConvert.DeserializeObject<IEnumerable<LogSession>>(File.ReadAllText(logSessionsFilePath)) :
                                                     Enumerable.Empty<LogSession>();

            if ((loggedSessions != null) && loggedSessions.Any())
            {
                sessions.ToList().AddRange(loggedSessions);
            }

            //sessions.Serialize(logSessionsFilePath);
            string json = JsonConvert.SerializeObject(sessions).FormatJsonString();
            File.WriteAllText(logSessionsFilePath, json);

            //Console.Write("Press any key to quit ...");
            //Console.ReadKey();
        }
    }
}
