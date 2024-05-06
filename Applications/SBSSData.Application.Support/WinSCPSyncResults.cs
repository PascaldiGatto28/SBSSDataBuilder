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
            string text =
                $"""
                 Uploaded files ({NumUploads}):
                 {Uploads.ToString<string>("\r\n")}
                 Downloaded files ({NumDownloads}):
                 {Downloads.ToString<string>("\r\n")}
                 {NumFailures} Failures 
                 Copy was {success}successful 
                 """;
            return text;
        }

    }
}
