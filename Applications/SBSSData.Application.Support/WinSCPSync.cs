using WinSCP;

namespace SBSSData.Application.Support
{
    public static class WinSCPSync
    {
        public static WinSCPSyncResults Sync(bool isTest = true)
        {
            // Set up session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = "ftp.walkingtree.com",
                UserName = "quietcre",
                Password = "85232WindingWay",
            };

            string target = isTest ? "/quietcre/TestSync" : "/quietcre/Data";
            SynchronizationResult? syncResult = default;

            using (Session session = new Session())
            {
                session.Open(sessionOptions);

                try
                {
                    syncResult = session.SynchronizeDirectories(SynchronizationMode.Remote,
                                                                @"J:\SBSSDataStore\HtmlData",
                                                                target,
                                                                false);
                    syncResult.Check();

                    //Console.WriteLine($"Failures {synchronizationResult.Failures.Count}");
                    //Console.WriteLine($"Uploads ({synchronizationResult.Uploads.Count})\r\n{synchronizationResult.Uploads.Select(t => t.FileName).ToString<string>("\r\n")}");
                    //Console.WriteLine($"Downloads {synchronizationResult.Downloads.Count}");
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Result Check Failure", exception);
                }
            }

            return new WinSCPSyncResults(syncResult);
        }
    }
}
