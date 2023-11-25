using Levaro.SBSoftball.Logging;

namespace Levaro.Application.SBSSDataStore
{
    internal class Program
    {
        internal static void Main()
        {
            // Assembly assembly = Assembly.GetExecutingAssembly();
            // string buildDate = assembly.GetFullVersion("{1};{2};{3};{4};{0:dddd, MMMM dd yyyy}").Split(';')[4];
            Console.WriteLine($"\r\nSBSS Data Store Manager -- Building and updating the SBSS data store (Built on {DateTime.Now:dddd MMMM d, yyyy})\r\n");

            AppContext context = AppContext.Instance;
            try
            {
                using ILog log = context.Log;
                log.WriteLine("Starting the Data store manager");
                try
                {
                    DataStoreManager.Run(false);
                }
                catch (Exception ex)
                {
                    log.WriteLine("Error not caught", ex);
                    throw new InvalidOperationException("Exception encountered while the data store manage is executing", ex);
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

        }
    }
}
