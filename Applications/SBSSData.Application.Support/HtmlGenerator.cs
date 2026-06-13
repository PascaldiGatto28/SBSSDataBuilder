using HtmlAgilityPack;
using LINQPad;
using SBSSData.Softball.Common;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Generates and post-processes HTML pages using a LINQPad XHTML writer and HtmlAgilityPack.
    /// Provides helpers to write text, raw HTML and table content, and to produce a final HTML document
    /// with optional head elements, CSS, JavaScript and table-collapsing behavior.
    /// </summary>
    public sealed class HtmlGenerator : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlGenerator"/> class.
        /// Creates the underlying LINQPad XHTML writer and initializes the table node callback list.
        /// </summary>
        public HtmlGenerator()
        {
            Writer = LINQPad.Util.CreateXhtmlWriter(true, 6, false);
            TableNodeCallbacks = [];
        }

        /// <summary>
        /// Gets the <see cref="TextWriter"/> used to write XHTML output.
        /// This writer is created by LINQPad and is disposed when <see cref="Dispose"/> is called.
        /// </summary>
        public TextWriter Writer
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the list of callbacks used to provide table header text for root tables.
        /// Each callback receives a <see cref="TableNode"/> and returns the header string to apply.
        /// Callbacks may be null to indicate no header is provided for a corresponding table.
        /// </summary>
        public List<Func<TableNode, string>?> TableNodeCallbacks
        {
            get;
            init;
        }

        /// <summary>
        /// Writes plain text to the underlying writer.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public void WriteText(string text)
        {
            Writer.Write(text);
        }

        /// <summary>
        /// Writes a simple text table (an object with an Information property) to the writer.
        /// Useful for producing a small one-column table-like display in the XHTML output.
        /// </summary>
        /// <param name="text">The text to include in the table's Information cell.</param>
        public void WriteTextTable(string text) //, string description = "", string header = "")
        {
            var displayObject = new
            {
                Information = text,
            };

            Writer.Write(displayObject); //, description: description, header: writeHeader);
        }

        /// <summary>
        /// Writes raw HTML to the output without HTML-encoding by wrapping the content with LINQPad's raw HTML helper.
        /// </summary>
        /// <param name="text">The raw HTML string to write.</param>
        public void WriteRawHtml(string text)
        {
            object displayObject = Util.RawHtml(text);
            Writer.Write(displayObject);
        }

        /// <summary>
        /// Writes an arbitrary object to the underlying writer.
        /// The writer determines how the object is rendered into XHTML.
        /// </summary>
        /// <param name="value">The object to write.</param>
        public void Write(object value)
        {
            Writer.Write(value);
        }

        /// <summary>
        /// Writes a root table value to the writer and registers an optional callback to provide a table header.
        /// </summary>
        /// <param name="value">The table root value to write.</param>
        /// <param name="callback">A callback that receives the corresponding <see cref="TableNode"/> and returns a header string, or <c>null</c>.</param>
        public void WriteRootTable(object? value, Func<TableNode, string>? callback)
        {
            Writer.Write(value);
            TableNodeCallbacks.Add(callback);
        }

        private static readonly string emptyDoc = """<html><body><span style="color:firebrick; font-size:1.50em; font-weight:bold;">This is an empty document; no tables were written.</span></body></html>""";

        /// <summary>
        /// Produces the final HTML document string after applying post-processing to the raw XHTML produced by the writer.
        /// </summary>
        /// <param name="pageTitle">An optional <see cref="HtmlNode"/> to prepend to the document body as the page title (can contain any HTML).</param>
        /// <param name="cssStyles">Optional additional CSS styles to append to the document's &lt;style&gt; element.</param>
        /// <param name="javaScript">Optional additional JavaScript to append to the document's &lt;script&gt; element.</param>
        /// <param name="collapseTo">
        /// Collapse depth threshold for nested tables. Tables with nesting level greater than or equal to this value
        /// will be collapsed (their tbody/tfoot hidden) and an arrow indicator adjusted. Default is 1.
        /// </param>
        /// <param name="headElements">Optional list of head elements to add (meta, link, title, etc.).</param>
        /// <returns>The processed document HTML as a string (outer HTML of the root node).</returns>
        /// <remarks>
        /// Uses HtmlAgilityPack to modify table headers, apply callbacks that set table header text, collapse nested tables,
        /// add head elements, and append CSS/JavaScript. If no content was written to the writer, returns a minimal empty-document HTML.
        /// </remarks>
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
                            string headerStyle = n.GetAttributeValue("style", null);
                            headerStyle = string.IsNullOrEmpty(headerStyle) ? "text-align:right" : $""""text-align:right; {headerStyle}"""";
                            n.Attributes.Add("style", headerStyle);
                        }

                        n.InnerHtml = n.InnerText.NameToTitle().Replace(" ", "&nbsp;");
                    }

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

                        // If there is no data, then the span element is null
                        if (span != null)
                        {
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
                                HtmlNode tableTfoot = table.SelectSingleNode("tfoot");
                                tableTfoot?.SetAttributeValue("style", "display:none");

                                HtmlNode columnHeaders = table.SelectSingleNode("thead/tr[2]");

                                // Some tables have just one header, and it is not represented in a row.
                                columnHeaders?.Attributes.Add("style", "display:none");
                            }

                            // Remove the footer
                            //HtmlNode tableFooter = table.SelectSingleNode("./tfoot");
                            //if (tableFooter != null)
                            //{
                            //    table.RemoveChild(tableFooter);
                            //}

                            // Finally, put TableNode information in the TableNode.TableHtmlNode so that it is available to
                            // the code (script) in the finally HTML document.

                            TableNode node = new(table);
                            span.Attributes.Add("depth", node.Depth().ToString());
                            span.Attributes.Add("index", node.Index().ToString());
                        }
                    }
                }
            }

            return rootNode.OuterHtml;
        }

        /// <summary>
        /// Adds the provided list of head elements (meta, link, title, etc.) to the specified document head node.
        /// Elements are prepended in the order they appear in the list.
        /// </summary>
        /// <param name="htmlDocument">The <see cref="HtmlDocument"/> used to create new head element nodes.</param>
        /// <param name="headNode">The document head <see cref="HtmlNode"/> to which elements will be added.</param>
        /// <param name="headElements">The list of <see cref="HeadElement"/> instances that describe head elements and attributes.</param>
        public static void AddHeadData(HtmlDocument htmlDocument, HtmlNode headNode, List<HeadElement> headElements)
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

        /// <summary>
        /// Disposes the underlying writer and releases any resources used by this instance.
        /// After calling <see cref="Dispose"/>, the writer should not be used.
        /// </summary>
        public void Dispose()
        {
            Writer.Dispose();
        }
    }
}