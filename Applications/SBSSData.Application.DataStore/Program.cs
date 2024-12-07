using SBSSData.Softball.Logging;

using AppContext = SBSSData.Application.Infrastructure.AppContext;

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
        ///         is invoked, passing an <see cref="SBSSData.Application.Infrastructure.AppSettings"/> 
        ///         property to it. The instance of the
        ///         <c>AppSettings</c> class is constructed by the <c>AppContext</c> when it is created.
        ///     </item>
        ///     <item>
        ///         When the <c>DataStoreManager</c> returns, and errors are caught and logged. The log file
        ///         is processed and serialized to a JSON file. Finally the Log is disposed to clean everything
        ///         up.
        ///     </item>
        /// </list>
        /// </remarks>
        public static void Main()
        {
            Console.WriteLine($"\r\nSBSS Data Store Manager —  Building and Updating the SBSS Data Store ({DateTime.Now:dddd MMMM d, yyyy})");
            Console.WriteLine("Version 1.12.24342 — Released Date Tuesday, September 3 2024\r\n");

            AppContext context = AppContext.Instance;
            try
            {
                using Log log = context.Log;
                HtmlDeployment htmlDeployment = new(log);
                log.WriteLine("Starting the Data Store Manager and HTML Deployment");
                try
                {
                    bool dsModified = DataStoreManager.Run((context.Settings).Update);

                    // If no items have been updated, then HTML files that depend on the changed data store will not be created.
                    htmlDeployment.CreateHtml(dsModified);
                }
                catch (Exception exception)
                {
                    log.WriteLine($"Error not caught -- {exception.Message}");
                    throw new InvalidOperationException("Exception encountered while the data store manager is executing", exception);
                }
                finally
                {
                    // This method deploys all the HTML files if requested (first parameter). To generate the LogSessions
                    // HTML file, the log must be closed and then generated the JSON file, build the LogSessions.html
                    // file and then it copies that single to the server. Even if no other HTML files are build or 
                    // deployed, the LogSessions file is always build and deployed.
                    htmlDeployment.FinishDeployment(true, false);
                }
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine($"Unhandled exception during Log processing {exception}");
            }
        }
    }
}
