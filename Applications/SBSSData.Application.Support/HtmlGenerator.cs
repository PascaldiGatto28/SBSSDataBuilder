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

        public string DumpHtml(HtmlNode? pageTitle = null, string cssStyles = "", string javaScript = "", int collapseTo = 1, List<HeadElement>? headElements = null)
        {
            string? docHtml = Writer?.ToString();
            HtmlNode rootNode = HtmlNode.CreateNode(emptyDoc);
            if (!string.IsNullOrEmpty(docHtml))
            {
                // Use HtmlAgilityPack to change the HTML. 

                // Add meta, link and title elements to the document head for this page.
                HtmlDocument htmlDocument = new();
                htmlDocument.LoadHtml(docHtml);
                rootNode = htmlDocument.DocumentNode;
                HtmlNode headNode = rootNode.SelectSingleNode("//head");
                if ((headElements != null) && (headElements.Count > 0))
                {
                    AddHeadData(htmlDocument, headNode, headElements);
                }

                HtmlNode style = rootNode.SelectSingleNode("//style");
                string styles = style.InnerHtml;
                if (!string.IsNullOrEmpty(cssStyles))
                {
                    styles += cssStyles;
                }

                style.InnerHtml = styles;

                HtmlNode script = rootNode.SelectSingleNode("//script");
                string scripts = script.InnerHtml;
                if (!string.IsNullOrEmpty(javaScript))
                {
                    scripts += javaScript;
                }

                script.InnerHtml = scripts;

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
                        HtmlNode span = table.SelectSingleNode("thead/tr/td/a/span");
                        if (nestingLevel >= collapseTo)
                        {
                            span.Attributes["class"].Value = "arrow-down";
                            string? styleAttribute = table.GetAttributeValue("style", null);
                            string styleSpec = "border-bottom-style: dashed;";
                            if (styleAttribute != null)
                            {
                                styleSpec += " " + styleAttribute;
                            }

                            table.SetAttributeValue("style", styleSpec);

                            HtmlNode tableTbody = table.SelectSingleNode("tbody");
                            tableTbody.SetAttributeValue("style", "display:none");
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

                        // Finally, put TableNode information in the TableNode.TableHtmlNode so that it is available to
                        // the code (script) in the finally HTML document.

                        TableNode node = new(table);
                        span.Attributes.Add("depth", node.Depth().ToString());
                        span.Attributes.Add("index", node.Index().ToString());
                    }
                }
            }

            return rootNode.OuterHtml;
        }

        private static void AddHeadData(HtmlDocument htmlDocument, HtmlNode headNode, List<HeadElement> headElements)
        {
            if ((htmlDocument != null) && (headNode != null))
            {
                foreach (HeadElement headElement in headElements)
                {
                    string elementName = headElement.ElementName;
                    HtmlNode elementNode = htmlDocument.CreateElement(headElement.ElementName);

                    if (elementName != "title")
                    {
                        foreach (string[] attribute in headElement.Attributes)
                        {
                            elementNode.SetAttributeValue(attribute[0], attribute[1]);
                        }
                    }
                    else
                    {
                        elementNode.InnerHtml = headElement.Attributes.First()[0];
                    }

                    headNode?.PrependChild(elementNode);
                }
            }
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
