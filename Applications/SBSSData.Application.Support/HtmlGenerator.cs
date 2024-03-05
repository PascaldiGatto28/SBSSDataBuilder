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
        }

        public TextWriter Writer
        {
            get;
            init;
        }

        public List<Func<TableNode, string>?> TableNodeCallbacks
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

            Writer.Write(displayObject); //, description: description, header: writeHeader);
        }
        public void WriteRawHtml(string text)
        {
            object displayObject = Util.RawHtml(text);
            Writer.Write(displayObject);
        }

        public void Write(object value)
        {
            Writer.Write(value);
        }

        public void WriteRootTable(object? value, Func<TableNode, string>? callback)
        {
            Writer.Write(value);
            TableNodeCallbacks.Add(callback);
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
                    // The page title is an HtmlNode, so can be any HTML and have any styling, not just text.
                    body.PrependChild(pageTitle);
                }

                // If tables is null, then no tables have been  written to the page.
                HtmlNodeCollection tables = body.SelectNodes("//table");
                //List<HtmlNode> rootTables = [];
                if ((tables != null) && (tables.Count != 0))
                {

                    // First right justify all the table column headers (th tags) so they are right-justified when the values 
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

                    //tables = rootNode.SelectNodes("//table");
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

                            // It is possible that no tables are specifying a callback, so don't do any thing.
                            Func<TableNode, string>? callback = (TableNodeCallbacks.Count != 0) ? TableNodeCallbacks[rootTableIndex++] : null;
                            if ((tableTree.Root != null) && (callback != null))
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
