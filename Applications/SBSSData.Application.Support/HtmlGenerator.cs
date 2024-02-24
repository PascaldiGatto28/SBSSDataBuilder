using HtmlAgilityPack;

using LINQPad;

using SBSSData.Softball.Common;

namespace SBSSData.Application.Support
{
    public sealed class HtmlGenerator : IDisposable
    {
        public HtmlGenerator()
        {
            Writer = LINQPad.Util.CreateXhtmlWriter(true, 6, false);
            TableNodeCallbacks = [];
            //OutputFolder = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\SBSSData-Application-Samples\HtmlOutput");
            //Descriptions = [];
            //Headers = [];
            //TableObjects = [];
        }

        //public HtmlGenerator(string? outputFolder = null) : this()
        //{
        //    OutputFolder = outputFolder ?? Path.GetTempPath();
        //    Directory.CreateDirectory(OutputFolder);
        //    Descriptions = [];
        //    Headers = [];
        //}

        public TextWriter Writer
        {
            get;
            init;
        }

        //public string OutputFolder
        //{
        //    get;
        //    set;
        //}

        //public List<string> Descriptions
        //{
        //    get;
        //    set;
        //}

        //public List<string> Headers
        //{
        //    get;
        //    set;
        //}

        //public Dictionary<Guid, object> TableObjects
        //{
        //    get;
        //    init;
        //}

        public List<Func<TableNode, string>> TableNodeCallbacks
        {
            get;
            init;
        }

        public void WriteText(string text)
        {
            Writer.Write(text);
        }

        public void WriteTextTable(string text) //, string description = "", string header = "")
        {
            var displayObject = new
            {
                Information = text,
            };

            //string writeHeader = !string.IsNullOrEmpty(header) ? header : displayObject.GetType().TypeToString();
            Write(displayObject); //, description: description, header: writeHeader);
        }
        public void WriteRawHtml(string text)
        {
            object displayObject = Util.RawHtml(text);
            //Headers.Add(WebUtility.HtmlEncode(header));
            Writer.Write(displayObject);
        }

        public void Write(object value, Func<TableNode, string>? callback = null)
        {
            Writer.Write(value);
            if (callback != null)
            {
                TableNodeCallbacks.Add(callback);
            }
        }

        private static readonly string emptyDoc = """<html><body><span style="color:firebrick; font-size:1.50em; font-weight:bold;">This is an empty document; no tables were written.</span></body></html>""";

        public string DumpHtml(HtmlNode? pageTitle = null, int collapseTo = 1)
        {
            string? docHtml = Writer?.ToString();
            HtmlNode rootNode = HtmlNode.CreateNode(emptyDoc);
            if (!string.IsNullOrEmpty(docHtml))
            {
                // Use HtmlAgilityPack to change the HTML. 

                HtmlDocument htmlDocument = new();
                htmlDocument.LoadHtml(docHtml);
                rootNode = htmlDocument.DocumentNode;

                // First add a CSS style so that column totals are not displayed.
                HtmlNode style = rootNode.SelectSingleNode("//style");
                string html = style.InnerHtml + "tr.columntotal {\r\nvisibility:none;\r\n}\r\n";
                style.InnerHtml = html;


                // Now if a description is specified, create the heading presenter container and include in the
                // document HTML.
                HtmlNode body = rootNode.SelectSingleNode("//body");

                if (pageTitle != null)
                {
                    // TODO: Allow styling of the title -- pass a HTML node or just the html???
                    //HtmlNode title = HtmlNode.CreateNode($"<div>{pageTitle}</div>");
                    body.PrependChild(pageTitle);
                }

                // If tables is null, then no tables have been  written to the page.
                HtmlNodeCollection tables = body.SelectNodes("//table");
                List<HtmlNode> rootTableS = [];
                if ((tables != null) && (tables.Count != 0))
                {
                    #region Headers and description code is no longer supported. See callback.
                    //for (int i = 0; i < Descriptions.Count; i++)
                    //{
                    //    string description = Descriptions[i];
                    //    if (!string.IsNullOrEmpty(description))
                    //    {
                    //        HtmlNode table = tables.ElementAt(i);
                    //        HtmlNode h1 = HtmlNode.CreateNode($"<h1 class=\"headingpresenter\">{description}</h1>");

                    //        // TODO Fix this. It fails if there is no previous description I think!!!
                    //        table.InsertBefore(h1, table);
                    //        table.Attributes["class"].Remove();
                    //        table.Attributes.Add("class", "headingpresenter");
                    //    }

                    //}

                    //// If a headers are specified, use those.
                    //HtmlNodeCollection currentHeaders = rootNode.SelectNodes("//table//tr/td[@class='typeheader']/a");

                    //for (int i = 0; i < Headers.Count; i++)
                    //{
                    //    HtmlNode currentHeader = currentHeaders[i];
                    //    //HtmlNode spanInHeader = currentHeader.SelectSingleNode("span");

                    //    // TODO: Change the header text. This is going to be a problem, I think if there are nested tables
                    //    string header = Headers[i];
                    //    if (!string.IsNullOrEmpty(header))
                    //    {
                    //        html = currentHeader.OuterHtml;
                    //        string text = html.Substring("</span>", "</a>", false, false);
                    //        html = html.Replace(text, header);
                    //        currentHeader.ParentNode.ReplaceChild(HtmlNode.CreateNode(html), currentHeader);
                    //    }
                    //}
                    #endregion

                    tables = rootNode.SelectNodes("//table");
                    int rootTableIndex = 0;
                    foreach (HtmlNode table in tables)
                    {
                        if (!table.Ancestors("table").Any())
                        {
                            TableNode rootTableNode = new(table);
                            TableTree tableTree = new();

                            foreach (HtmlNode tableHtmlNode in table.DescendantsAndSelf("table"))
                            {
                                TableNode tableNode = new(tableHtmlNode);
                                tableTree.Insert(tableNode);
                            }

                            Func<TableNode, string> callback = TableNodeCallbacks[rootTableIndex++];
                            if (tableTree.Root != null)
                            {
                                TableTree.SetTableHeader(tableTree.Root, callback);
                            }
                        }

                        // Now collapse all tables whose depth is greater than or equal to the collapseTo parameter
                        int nestingLevel = GetTableNestingLevel(table);
                        if (nestingLevel >= collapseTo)
                        {
                            HtmlNode span = table.SelectSingleNode("thead/tr/td/a/span");
                            span.Attributes["class"].Value = "arrow-down";
                            HtmlNode tableTBody = table.SelectSingleNode("tbody");
                            tableTBody.Attributes.Add("style", "display:none");
                            HtmlNode columnHeaders = table.SelectSingleNode("thead/tr[2]");

                            // Some tables have just one header, and it is not represented in a row.
                            columnHeaders?.Attributes.Add("style", "display:none");
                        }

                        // Remove the footer
                        HtmlNode tableFooter = table.SelectSingleNode("./tfoot");
                        if (tableFooter != null)
                        {
                            table.RemoveChild(tableFooter);
                        }
                    }


                    // Finally right justify all the table column headers (th tags) so they are right-justified when the values 
                    // are numeric (int or double). Also change the column to words, for example "BuildDate" becomes "Build Date".
                    HtmlNodeCollection tableColumnHeaders = rootNode.SelectNodes("//table//tr//th");
                    foreach (HtmlNode n in tableColumnHeaders)
                    {
                        string columnHeaderTitle = n.GetAttributeValue("title", string.Empty);
                        if ((columnHeaderTitle == "System.Int32") || (columnHeaderTitle == "System.Double"))
                        {
                            n.Attributes.Add("style", "text-align:right");
                        }

                        n.InnerHtml = n.InnerText.NameToTitle().Replace(" ", "&nbsp;");
                    }
                }

            }

            return rootNode.OuterHtml;
        }

        private static int GetTableNestingLevel(HtmlNode table)
        {
            int nestingLevel = 0;
            HtmlNode parent = table.ParentNode;
            while (parent != null)
            {
                if (parent.Name == "table")
                {
                    nestingLevel++;
                }
                parent = parent.ParentNode;
            }
            return nestingLevel;
        }

        public void Dispose()
        {
            Writer.Dispose();
        }
    }
}
