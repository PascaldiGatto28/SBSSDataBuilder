using HtmlAgilityPack;

using Levaro.SBSoftball.Common;

namespace Levaro.SBSoftball
{
    /// <summary>
    /// Utilities that are useful to recover (scrape) information form HTML pages.
    /// </summary>
    /// <remarks>
    /// The are just two categories of utilities, the first gathers the raw HTML from the page. The second using the
    /// <see cref="HtmlAgilityPack"/> API to return <see cref="HtmlAgilityPack.HtmlDocument"/> objects after parsing the
    /// HTML while then allows to recover information that is structured.
    /// <para>
    /// Because these utilities are so specialized for the classes in the <see cref="Levaro.SBSoftball"/> namespace, none of
    /// them have been packaged as extensions, but rather just static methods.
    /// </para>
    /// </remarks>
    public static class PageContentUtilities
    {
        /// <summary>
        /// Asynchronously returns the contents of an HTML page as array of bytes.
        /// </summary>
        /// <remarks>
        /// No error checking is done, so a number of exception can be thrown.
        /// </remarks>
        /// <param name="requestAddress">The request address of the page as a string, representing the URL location
        /// of the page.
        /// </param>
        /// <returns>The page source as and array of bytes. The array may be empty, but never <c>null</c>.</returns>
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

        /// <summary>
        /// Returns the contents of an HTML page as array of bytes.
        /// </summary>
        /// <remarks>
        /// This is identical to <see cref="DownloadPageDataAsync(string)"/> except it is synchronous and does capture any
        /// thrown exceptions and wraps <see cref="InvalidOperationException"/>
        /// </remarks>
        /// <param name = "requestAddress" > The request address of the page as a string, representing the URL location
        /// of the page.
        /// </param>
        /// <returns>The page source as and array of bytes. The array may be empty, but never <c>null</c>.</returns>
        /// <exception cref="InvalidOperationException">when any <see cref="AggregateException"/> is encountered and
        /// construct a message containing all the inner exception messages.</exception>
        /// <seealso cref="DownloadPageDataAsync(string)"/>
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

        /// <summary>
        /// Returns the contents of an HTML page as a string.
        /// </summary>
        /// <remarks>
        /// This method simply uses <see cref="GetPageData(string)"/> and converts the byte array to a string.
        /// </remarks>
        /// <param name = "requestAddress" > The request address of the page as a string, representing the URL location
        /// of the page.
        /// </param>
        /// <returns>A string representing the contents of the HTML page. It can be empty, but it's never <c>null</c>.</returns>
        /// <seealso cref="GetPageData(string)"/>
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

        /// <summary>
        /// Gets the <see cref="HtmlAgilityPack.HtmlDocument"/> object for the HTML page.
        /// </summary>
        /// <param name="requestAddress">The <see cref="Uri"/> of the page.</param>
        /// <returns>An <c>HtmlDocument</c> that can be empty but not <c>null</c>.</returns>
        public static HtmlDocument GetPageHtmlDocument(Uri requestAddress)
        {
            HtmlDocument htmlDocument = new();
            if (requestAddress != null)
            {
                HtmlWeb web = new();
                htmlDocument = web.Load(requestAddress);
            }

            return htmlDocument;
        }

        /// <summary>
        /// Gets the <see cref="HtmlAgilityPack.HtmlDocument"/> object for an HTML document.
        /// </summary>
        /// <param name="filePath">The full file path used to recover the HTML contents.</param>
        /// <returns>An <c>HtmlDocument</c> that can be empty but not <c>null</c>.</returns>
        public static HtmlDocument GetPageHtmlDocument(string filePath)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.Load(filePath);
            return htmlDoc;
        }

        /// <summary>
        /// Gets the <see cref="HtmlAgilityPack.HtmlDocument"/> object for text.
        /// </summary>
        /// <param name="htmlText">The full file path used to recover the HTML contents.</param>
        /// <returns>An <c>HtmlDocument</c> that can be empty, especially if the text is not well-formed HTML,
        /// but not <c>null</c>.
        /// </returns>
        public static HtmlDocument GetHtmlDocument(string htmlText)
        {
            HtmlDocument htmlDoc = new();
            htmlDoc.LoadHtml(htmlText);
            return htmlDoc;
        }

        /// <summary>
        /// Gets the <see cref="HtmlAgilityPack.HtmlDocument"/> object associated with an <see cref="HtmlAgilityPack.HtmlNode"/>.
        /// </summary>
        /// <param name="node">The <c>HtmlNode</c> from which to recover its structured returned as an <c>HtmlDocument</c>.</param>
        /// <param name="justChildNodes">If <c>true</c>, just the inner HTML of the <c>HtmlNode</c> is parsed and returned.
        /// If <c>false</c>, the node itself and all children are parsed (that is, the outer HTML). This parameter is
        /// optional and the default value is <c>false</c>.</param>
        /// <returns>
        /// An <c>HtmlDocument</c> that can be empty but not <c>null</c>.
        /// </returns>
        public static HtmlDocument GetHtmlDocument(HtmlNode node, bool justChildNodes = false)
        {
            return GetHtmlDocument(justChildNodes ? node.InnerHtml : node.OuterHtml);
        }

        /// <summary>
        /// Gets the <see cref="HtmlAgilityPack.HtmlDocument"/> object associated with an <see cref="HtmlAgilityPack.HtmlNode"/>
        /// having a specified name..
        /// </summary>
        /// <remarks>
        /// This method just uses <see cref="GetHtmlDocument(HtmlNode, bool)"/> after finding the <c>HtmlNode</c> in the 
        /// document with the specified name.
        /// </remarks>
        /// <param name="htmlDocument">The HTML document to search of the <c>HtmlNode</c> having <paramref name="nodeName"/>
        /// name.</param>
        /// <param name="nodeName">The <c>HtmlNode</c> with this name from which to recover its structured 
        /// HTML returned as an <c>HtmlDocument</c>.</param>
        /// <param name="justChildNodes">If <c>true</c>, just the inner HTML of the <c>HtmlNode</c> is parsed and returned.
        /// If <c>false</c>, the node itself and all children are parsed (that is, the outer HTML). This parameter is
        /// optional and the default value is <c>false</c>.</param>
        /// <returns>
        /// An <c>HtmlDocument</c> that can be empty but not <c>null</c>.
        /// </returns>
        public static HtmlDocument GetHtmlDocument(HtmlDocument htmlDocument, string nodeName, bool justChildNodes = false)
        {
            HtmlNode node = htmlDocument.DocumentNode.SelectSingleNode($"//{nodeName}");
            return GetHtmlDocument(node, justChildNodes);
        }
    }
}
