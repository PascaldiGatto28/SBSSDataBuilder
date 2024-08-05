using SBSSData.Softball.Common;

using WinSCP;

namespace SBSSData.Application.Support
{
    public record WinSCPSyncResults(IEnumerable<string> Uploads, IEnumerable<string> Downloads, int NumFailures, bool IsSuccess)
    {
        public WinSCPSyncResults(SynchronizationResult results) : this(results?.Uploads.Select(t => t.FileName) ?? [],
                                                                       results?.Downloads.Select(t => t.FileName) ?? [],
                                                                       results?.Failures.Count ?? 0,
                                                                       results?.IsSuccess ?? false)

        {

        }

        public int NumUploads = Uploads.Count();
        public int NumDownloads = Downloads.Count();

        public override string ToString()
        {
            string success = IsSuccess ? string.Empty : "not ";
            string text = "No files were deployed, because none have changed";
            switch (NumUploads)
            {
                case 0:
                {
                    if (!IsSuccess)
                    {
                        text = "No files were deployed, because the deployment was not successful";
                    }
                    break;
                }
                case 1:
                {
                    text =
                        $"""
                         One files deployed:
                         {Uploads.First()}
                         {NumDownloads} downloaded files
                         Deployment was {success}successful 
                         """;
                    break;
                }
                default:
                {
                    text =
                        $"""
                         {NumUploads} files deployed:
                         {Uploads.ToString<string>("\r\n")}
                         {NumDownloads} downloaded files
                         Deployment was {success}successful 
                         """;
                    break;
                }
            }
            return text;
        }

    }
}
