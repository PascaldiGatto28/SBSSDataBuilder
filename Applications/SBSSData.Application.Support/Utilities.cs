using System.Text;

using HtmlAgilityPack;

using SBSSData.Softball.Common;

using WinSCP;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Support extensions and methods for applications when displaying information, particularly in Web pages and
    /// LINQPad queries.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Returns a string representing the <see cref="Type"/> name.
        /// </summary>
        /// <remarks>
        /// For simple types, the <see cref="Type.Name"/> property is sufficient, however for generic and anonymous types the
        /// name is not very useful for display. For example, for the following two classes, the display of the class names
        /// <code language="cs">
        /// typeof(Foobar<IEnumerable<int>>).Name;
        /// var x = new
        /// {
        ///    Name = "Richard",
        ///    Y = 23,
        ///    Address = new { Point1 = new Point(0, 0), Point2 = new Point(1, 1) },
        ///    Foobar = new Foobar<IEnumerable<string>>()
        /// };
        /// x.GetType().Name;
        /// </code>
        /// produce the following
        /// <code language="cs">
        /// Foobar`1
        /// <>f__AnonymousType0`4
        /// </code>
        /// where as using the extension method to display the names 
        /// <code language="cs">
        /// x.GetType().TypeToString();
        /// typeof(Foobar<IEnumerable<int>>).TypeToString();
        /// </code>
        /// produce the following
        /// <code language="cs">
        /// Foobar<IEnumerable<Int32>>
        /// Anonymous<String, Int32, Anonymous<Point, Point>, Foobar<IEnumerable<String>>>
        /// </code>
        /// </remarks>
        /// <param name="type">The <see cref="Type"/> source of the extension method, or the first parameter is calling 
        /// directly.</param>
        /// <param name="typeString">An initial string that is appended to as embedded types are encountered. This is an
        /// optional parameter that normally is only used by the method itself during the recursion process if necessary.</param>
        /// <returns>
        /// A readable string representing the name of the type.
        /// </returns>
        public static string TypeToString(this Type type, string typeString = "")
        {
            string display = typeString;
            Type t = type.UnderlyingSystemType;
            bool isAnonymous = t.Name.StartsWith("<>f__AnonymousType");
            string name = isAnonymous ? "Anonymous" : t.Name;
            display += name;
            if (t.IsGenericType && !isAnonymous)
            {
                display = display[..display.IndexOf('`')];
                display = ProcessGenericTypeArgs(display, t);
            }
            else if (isAnonymous)
            {
                display = ProcessGenericTypeArgs(display, t);
            }
            return display;
        }

        /// <summary>
        /// Help method for <see cref="TypeToString"/> extension method to processes type arguments for generic classes.
        /// </summary>
        /// <remarks>
        /// This method calls TypeToString which recursively call this method to handle the case when the type arguments may 
        /// themselves be generic (anonymous classes are considered generic by the C# compiler).
        /// </remarks>
        /// <param name="display">The current readable display name for the parent type and is the return value can be
        /// appended to the processed name.</param>
        /// <param name="t">The <see cref="Type"/> of the generic type argument to process</param>
        /// <returns>The readable display name for the argument type that becomes part of the display name for its parent
        /// type.</returns>
        private static string ProcessGenericTypeArgs(string display, Type t)
        {
            display += "<";

            int length = t.GenericTypeArguments.Length;
            for (int i = 0; i < length; i++)
            {
                if (i > 0)
                {
                    display += ", ";
                }

                Type s = t.GenericTypeArguments[i];
                display = TypeToString(s, display);
                if (i == length - 1)
                {
                    display += ">";
                }
            }

            return display;
        }

        /// <summary>
        /// Moves any changes from the local to the remove ftp.walkingtree.com Data folder. This is the location
        /// of the sbssdata.info Web site.
        /// </summary>
        /// <param name="source">The local folder where the Web site files are located. The default is 
        /// J:\SBSSDataStore\HtmlData
        /// </param>
        /// <param name="isTest">If <c>true</c>, the target folder is "/quietcre/TestSync" (a test site); otherwise
        /// is is "/quietcre/Data". In the first case you need to access the text site via walkingtree.com/TestSync,
        /// in the production, you can use sbssdata URL, sbssdata.info.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If any failures, it is translated to an exceptions of this
        /// type. This is done so consumers of this method do not need to reference the WinSCP package directly.</exception>
        public static WinSCPSyncResults PublishSBSSData(string source = @"J:SBSSDataStore\HtmlData\", bool isTest = true)
        {
            // Set up session options
            SessionOptions sessionOptions = new()
            {
                Protocol = Protocol.Ftp,
                HostName = "ftp.walkingtree.com",
                UserName = "quietcre",
                Password = "85232WindingWay",
            };

            string target = isTest ? "/quietcre/TestSync" : "/quietcre/Data";
            SynchronizationResult? syncResult = default;

            using (Session session = new())
            {
                session.Open(sessionOptions);

                try
                {
                    syncResult = session.SynchronizeDirectories(SynchronizationMode.Remote,
                                                                source,
                                                                target,
                                                                false);
                    syncResult.Check();
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException("Result Check Failure", exception);
                }
            }

            return new WinSCPSyncResults(syncResult);
        }

        public static string PublishSingleFile(string localFilePath, bool isTest = true)
        {
            List<string> transferResults = [];
            try
            {
                // Setup session options
                SessionOptions sessionOptions = new()
                {
                    Protocol = Protocol.Ftp,
                    HostName = "ftp.walkingtree.com",
                    UserName = "quietcre",
                    Password = "85232WindingWay",
                };

                string target = isTest ? "/quietcre/TestSync/" : "/quietcre/Data/";
                string localFileName = Path.GetFileName(localFilePath);
                using (Session session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    // Upload file
                    TransferOptions transferOptions = new TransferOptions();
                    transferOptions.TransferMode = TransferMode.Binary;

                    TransferOperationResult transferResult = session.PutFiles(localFilePath, $"{target}{localFileName}", false, transferOptions);

                    // Throw on any error
                    transferResult.Check();

                    foreach (TransferEventArgs transfer in transferResult.Transfers)
                    {
                        transferResults.Add($"Upload of {transfer.FileName} succeeded");
                    }
                }
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException("Error when copying {localFilePath} to sbssdata.info",  exception);
            }

            return transferResults.ToString<string>("\r\n");
        }

        public static string SwapSeasonText(string seasonText)
        {
            string[] parts = seasonText.Split(' ');
            return $"{parts[1]} {parts[0]}";
        }

        #region Utilities to help in parsing LINQPad generated HTML pages and to modify the HTML
        public static bool IsRootTableNode(this HtmlNode tableNode)
        {
            bool isRoot = false;
            if (tableNode != null)
            {
                isRoot = !tableNode.Ancestors("table").Any();
            }

            return isRoot;
        }

        public static void AlterTableColumnHeader(HtmlNode tableNode, int headerIndex, string newHeaderText)
        {
            List<HtmlNode> columnHeaders = [.. tableNode.SelectNodes("./thead/tr[2]/th")];
            // This is a real pain in the ass. Because we searching a C# list, the first element is 0-based. But the XPath
            // is 1-based, so we really want the index that is 1-less.
            HtmlNode columnHeader = columnHeaders[headerIndex - 1];
            HtmlNode newHeader = HtmlNode.CreateNode(columnHeader.OuterHtml.Replace(columnHeader.InnerText, newHeaderText));
            columnHeader.ParentNode.ReplaceChild(newHeader, columnHeader);
        }

        public static int GetTableColumnIndex(HtmlNode tableNode, string columnHeaderText)
        {
            int index = -1;
            if ((tableNode != null) && !string.IsNullOrEmpty(columnHeaderText))
            {
                // LINQ is 0-based, but HtmlAgilityPack is 1-based, that is the first cell is td[1] or th[1].
                IEnumerable<HtmlNode> headers = tableNode.SelectNodes("./thead/tr[2]/th");
                if (headers != null)
                {
                    List<string> columnHeaders = headers.Select(n => n.InnerText).ToList();
                    index = columnHeaders.IndexOf(columnHeaderText) + 1;
                }
            }

            return index;
        }

        /// <summary>
        /// Hides the specified column (that is, adds a "display:none" style)
        /// </summary>
        /// <param name="tableNode"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public static HtmlNode? ExcludeTableColumn(HtmlNode tableNode, int columnIndex)
        {
            if ((tableNode != null) && (columnIndex > 0))
            {
                tableNode.SelectSingleNode($"./thead/tr[2]/th[{columnIndex}]").Attributes.Add("style", "display:none");
                tableNode.SelectNodes($"./tbody/tr/td[{columnIndex}]").ToList().ForEach(n => n.Attributes.Add("style", "display:none"));
            }

            return tableNode;
        }

        public static HtmlNode? ExcludeTableColumn(HtmlNode tableNode, string columnHeaderText)
        {
            int columnIndex = GetTableColumnIndex(tableNode, columnHeaderText);
            return ExcludeTableColumn(tableNode, columnIndex);
        }

        public static string DisplayTree(TableNode node, StringBuilder sbTree)
        {
            sbTree.AppendLine(node.ToString());
            foreach (TableNode childNode in node.ChildNodes)
            {
                DisplayTree(childNode, sbTree);
            }

            return sbTree.ToString();
        }

        //public static TableTree BuildTree(HtmlNode tableRootNode)
        //{
        //    TableTree tableTree = new();
        //    foreach (HtmlNode tableHtmlNode in tableRootNode.DescendantsAndSelf("table"))
        //    {
        //        TableNode tableNode = new(tableHtmlNode);
        //        tableTree.Insert(tableNode);
        //    }

        //    return tableTree;
        //}

        public static void UpdatePlayerColumnNames(HtmlNode tableHtmlNode)
        {
            int columnIndex = Utilities.GetTableColumnIndex(tableHtmlNode, "Singles");
            if (columnIndex != -1)
            {
                for (int i = columnIndex; i < (columnIndex + 3); i++)
                {
                    Utilities.AlterTableColumnHeader(tableHtmlNode, i, $"{i - (columnIndex - 1)}B");
                }
            }
        }

        public static List<HtmlNode> GetTableColumnHeaders(HtmlNode tableHtmlNode)
        {
            HtmlNodeCollection tableColumnHeaders = tableHtmlNode.SelectNodes("./thead/tr[2]//th");
            tableColumnHeaders ??= tableHtmlNode.SelectNodes("./tbody//tr/th");

            return [.. tableColumnHeaders];
        }

        public static string ToSpanItalic(string text)
        {
            return $"""<span style="font-style:italic">{text}</span>""";
        }

        public static void UpdatePlayerHeaderTitles(HtmlNode tableHtmlNode, 
                                                        bool includeGames, 
                                                        bool includeRankings = false,
                                                        bool includeZScores = false)
        {
            List<HtmlNode> tableColumnHeaders = GetTableColumnHeaders(tableHtmlNode);

            int numTitles = playerTitles.Count;
            //int numHeaders = Math.Min(tableColumnHeaders.Count, numTitles);

            List<string> titles = playerTitles;
            if (!includeZScores)
            {
                // Remove the last 4 titles which are the z-Scores titles
                titles = [.. titles.Take(numTitles - 4)];
            }
            if (!includeRankings)
            {
                // The ranking title is at the 18th location, so take the first 17 titles
                IEnumerable<string> beforeRanking = titles.Take(17);

                // Take the remainder of the titles after the ranking
                IEnumerable<string> afterRanking = titles.Skip(18);

                // Now put them together
                titles = beforeRanking.Concat(afterRanking).ToList();
            }
            if (!includeGames)
            {
                // Just use the titles after the number of games 
                titles = titles.Skip(1).ToList();
            }

            for (int i = 0; i < titles.Count;  i++)
            {
                tableColumnHeaders[i].SetAttributeValue("title", titles[i]);
            }

            //Console.WriteLine(tableColumnHeaders.Select(t => t.GetAttributeValue("title", null)).ToString<string>(", "));
            //return tableHtmlNode;
        }

        public static readonly List<string> playerTitles =
                          [
                            "Number of games played",
                            "Player name",
                            "Plate appearances",
                            "Official at bats",
                            "Runs",
                            "Singles",
                            "Doubles",
                            "Triples",
                            "Home Runs",
                            "Bases on Balls (walks)",
                            "Sacrifice Files",
                            "Total Hits = sum of singles, doubles, triples and home runs",
                            "Total Bases = singles + 2*doubles + 3*triples + 4*home runs",
                            "Average = TH / AB",
                            "Slugging = TB / AB",
                            "On-Base Percentage = Sum of total hits and walks divided by the plate appearances; (tH+BB)/PA",
                            "On-Base Plus Slugging = Sum of On-base percentage and slugging; OBP + SLG",
                            "Rankings for each player for AVG, SLG, OBP, OPS",
                            "The number of StdDevs above or below the league mean for AVG",
                            "The number of StdDevs above or below the league mean for SLG",
                            "The number of StdDevs above or below the league mean for OBP",
                            "The number of StdDevs above or below the league mean for OPS"
                          ];
        #endregion

    }
}
