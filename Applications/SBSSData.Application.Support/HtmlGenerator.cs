using System.Net;

using HtmlAgilityPack;

using SBSSData.Softball.Common;

namespace SBSSData.Application.Support
{
    public sealed class HtmlGenerator : IDisposable
    {
        public HtmlGenerator()
        {
            Writer = LINQPad.Util.CreateXhtmlWriter(true, 6, false);
            OutputFolder = Environment.ExpandEnvironmentVariables(@"%LocalAppData%\SBSSData-Application-Samples\HtmlOutput");
            Descriptions = [];
            Headers = [];
        }

        public HtmlGenerator(string outputFolder) : this()
        {
            OutputFolder = outputFolder ?? @"D:\Temp\";
            Directory.CreateDirectory(OutputFolder);
            Descriptions = [];
            Headers = [];
        }

        public TextWriter Writer
        {
            get;
            init;
        }

        public string OutputFolder
        {
            get;
            set;
        }

        public List<string> Descriptions
        {
            get;
            set;
        }

        public List<string> Headers
        {
            get;
            set;
        }

        public void WriteText(string text)
        {
            Writer.Write(text);
        }

        public void WriteTextTable(string text, string description = "", string header = "")
        {
            var displayObject = new
            {
                Information = text,
            };

            string defaultHeader = !string.IsNullOrEmpty(header) ? header : displayObject.GetType().TypeToString();
            Write(displayObject, description: description, header: defaultHeader);
        }

        public void Write(object value, string description = "", string header = "")
        {
            Descriptions.Add(WebUtility.HtmlEncode(description));
            Headers.Add(WebUtility.HtmlEncode(header));
            Writer.Write(value);
        }

        public string DumpHtml(string fileName, string pageTitle = "", int collapseTo = 1)
        {
            string? docHtml = Writer?.ToString();

            if (!string.IsNullOrEmpty(docHtml))
            {
                // Use HtmlAgilityPack to change the HTML. 

                HtmlDocument htmlDocument = new();
                htmlDocument.LoadHtml(docHtml);
                HtmlNode rootNode = htmlDocument.DocumentNode;

                // First add a CSS style so that column totals are not displayed.
                HtmlNode style = rootNode.SelectSingleNode("//style");
                string html = style.InnerHtml + "tr.columntotal {\r\ndisplay:none;\r\n}\r\n";
                style.InnerHtml = html;


                // Now if a description is specified, create the heading presenter container and include in the
                // document HTML.
                HtmlNode body = rootNode.SelectSingleNode("//body");

                if (!string.IsNullOrEmpty(pageTitle))
                {
                    HtmlNode title = HtmlNode.CreateNode($"<div>{pageTitle}</div>");
                    body.PrependChild(title);
                }

                HtmlNodeCollection spacers = body.SelectNodes("//div[@class='spacer']");
                for (int i = 0; i < Descriptions.Count; i++)
                {
                    string description = Descriptions[i];
                    if (!string.IsNullOrEmpty(description))
                    {
                        HtmlNode spacer = spacers[i];
                        HtmlNode table = spacer.SelectSingleNode("table");
                        HtmlNode h1 = HtmlNode.CreateNode($"<h1 class=\"headingpresenter\">{description}</h1>");
                        spacer.InsertBefore(h1, table);
                        spacer.Attributes["class"].Remove();
                        spacer.Attributes.Add("class", "headingpresenter");
                    }

                }

                // If a headers are specified, use those.
                HtmlNodeCollection currentHeaders = rootNode.SelectNodes("//table//tr/td[@class='typeheader']/a");
                for (int i = 0; i < Headers.Count; i++)
                {
                    HtmlNode currentHeader = currentHeaders[i];
                    //HtmlNode spanInHeader = currentHeader.SelectSingleNode("span");

                    // TODO: Change the header text. This is going to be a problem, I think if there are nested tables
                    string header = Headers[i];
                    if (!string.IsNullOrEmpty(header))
                    {
                        html = currentHeader.OuterHtml;
                        string text = html.Substring("</span>", "</a>", false, false);
                        html = html.Replace(text, header);
                        currentHeader.ParentNode.ReplaceChild(HtmlNode.CreateNode(html), currentHeader);
                    }

                    // Put the table in collapsed mode
                    //
                }

                // Now collapse all tables whose depth is greater than or equal to the collapseTo parameter
                HtmlNodeCollection tables = rootNode.SelectNodes("//table");
                foreach (HtmlNode table in tables)
                {
                    int nestingLevel = GetTableNestingLevel(table);
                    if (nestingLevel >= collapseTo)
                    {
                        HtmlNode span = table.SelectSingleNode("thead/tr/td/a/span");
                        span.Attributes["class"].Value = "arrow-down";
                        HtmlNode tableTBody = table.SelectSingleNode("tbody");
                        tableTBody.Attributes.Add("style", "display:none");
                        HtmlNode columnHeaders = table.SelectSingleNode("thead/tr[2]");

                        // Some tables have just one header, and it is not represented in a row.
                        if (columnHeaders != null)
                        {
                            columnHeaders.Attributes.Add("style", "display:none");
                        }
                    }
                }


                // Finally right justify all the table headers (th tags) so they are right-justified when the values are
                // numeric (int or double).
                HtmlNodeCollection tableColumnHeaders = rootNode.SelectNodes("//table//tr//th");
                tableColumnHeaders.Where(n => ((n.GetAttributeValue("title", string.Empty) == "System.Int32") ||
                                              (n.GetAttributeValue("title", string.Empty) == "System.Double")))
                                  .ToList()
                                  .ForEach(n => n.Attributes.Add("style", "text-align:right"));

                // Get the HTML page and persisted it to a file
                docHtml = rootNode.OuterHtml;
                string path = Path.Combine(OutputFolder, fileName);
                File.WriteAllText(path, docHtml);
            }
            else
            {
                throw new InvalidOperationException($"The output file {fileName} not written, because the data is null or empty.");
            }

            return docHtml;
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
