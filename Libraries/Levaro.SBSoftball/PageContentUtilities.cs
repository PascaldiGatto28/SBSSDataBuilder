using HtmlAgilityPack;

using Levaro.SBSoftball.Common;

namespace Levaro.SBSoftball
{
    public static class PageContentUtilities
    {
        public static async Task<byte[]> DownloadPageDataAsync(string requestAddress)
        {
            byte[] pageData = Array.Empty<byte>();
            using (HttpClient client = new())
            {
                pageData = await client.GetByteArrayAsync(requestAddress)
                                       .ConfigureAwait(false);
            }

            return pageData;
        }

        public static byte[] GetPageData(string requestAddress)
        {
            byte[] pageData;
            try
            {
                pageData = DownloadPageDataAsync(requestAddress).Result;
            }
            catch (AggregateException exception)
            {
                string message = $"{exception.GetType().Name}:{exception.Message}";
                Exception? innerException = exception.InnerException;
                while (innerException != null)
                {
                    message += $"\r\n    {innerException.GetType().Name}:{innerException.Message}";
                    innerException = innerException.InnerException;
                }

                throw new InvalidOperationException(message, exception);
            }

            return pageData;
        }

        public static string GetPageContents(string requestAddress)
        {
            string pageContents = string.Empty;
            if (!string.IsNullOrEmpty(requestAddress))
            {
                byte[] data = GetPageData(requestAddress);
                pageContents = data.ByteArrayToString();
            }

            return pageContents;
        }

        public static HtmlDocument GetPageHtmlDocument(Uri requestAddress)
        {
            HtmlDocument htmlDocument = new HtmlDocument();
            if (requestAddress != null)
            {
                HtmlWeb web = new();
                htmlDocument = web.Load(requestAddress);
            }

            return htmlDocument;
        }

        public static HtmlDocument GetPageHtmlDocument(string filePath)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.Load(filePath);
            return htmlDoc;
        }

        public static HtmlDocument GetHtmlDocument(string htmlText)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(htmlText);
            return htmlDoc;
        }

        public static HtmlDocument GetHtmlDocument(HtmlNode node, bool justChildNodes = false)
        {
            return GetHtmlDocument(justChildNodes ? node.InnerHtml : node.OuterHtml);
        }

        public static HtmlDocument GetHtmlDocument(HtmlDocument htmlDocument, string nodeName, bool justChildNodes = false)
        {
            HtmlNode node = htmlDocument.DocumentNode.SelectSingleNode($"//{nodeName}");
            return GetHtmlDocument(node, justChildNodes);
        }
    }
}
