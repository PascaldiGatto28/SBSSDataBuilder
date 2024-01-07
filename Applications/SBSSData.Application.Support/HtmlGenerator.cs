using HtmlAgilityPack;

using SBSSData.Softball.Common;

namespace SBSSData.Application.Support
{
    public sealed class HtmlGenerator : IDisposable
    {
        public HtmlGenerator()
        {
            Writer = LINQPad.Util.CreateXhtmlWriter(true, 6, false);
            OutputFolder = Environment.ExpandEnvironmentVariables(@"LocalAppData\SBSSData-Application-Samples\HtmlOutput");
        }

        public HtmlGenerator(string outputFolder) : this()
        {
            OutputFolder = outputFolder ?? @"D:\Temp\";
            Directory.CreateDirectory(OutputFolder);
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

        public void Write(object value)
        {
            Writer.Write(value);
        }

        public string DumpHtml(string fileName, string[] description = null, string[] headers = null)
        {
            string? docHtml = Writer?.ToString();

            string path = Path.Combine(OutputFolder, fileName);
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


                // Now if a description is specified, create the heading presenter container and include int the
                // document HTML.
                if ((description != null) && description.Any())
                {
                    HtmlNode body = rootNode.SelectSingleNode("//body");
                    HtmlNodeCollection spacers = body.SelectNodes("//div[@class='spacer']");
                    int numSpaces = description.Length;
                    for (int i = 0; i < numSpaces; i++)
                    {
                        HtmlNode spacer = spacers[i];
                        HtmlNode table = spacer.SelectSingleNode("table");
                        string id = table.Attributes["id"].Value;

                        string descText = string.IsNullOrEmpty(description[i]) ? $"Table {id}" : description[i];

                        HtmlNode h1 = HtmlNode.CreateNode($"<h1 class=\"headingpresenter\">{descText}</h1>");
                        spacer.InsertBefore(h1, spacer.SelectSingleNode("table"));
                        spacer.Attributes["class"].Remove();
                        spacer.Attributes.Add("class", "headingpresenter");
                    }
                }

                // If a headers are specified, use those.
                if ((headers != null) && headers.Any())
                {
                    HtmlNodeCollection currentHeaders = rootNode.SelectNodes("//table/tr/td[@class='typeheader']/a");
                    int numHeaders = headers.Length;
                    for (int i = 0; i < numHeaders; i++)
                    {
                        HtmlNode header = currentHeaders[i];
                        html = header.OuterHtml;
                        string text = html.Substring("</span>", "</a>", false, false);
                        html = html.Replace(text, headers[i]);
                        header.InnerHtml = html;
                    }
                }

                docHtml = rootNode.OuterHtml;

                File.WriteAllText(path, docHtml);
            }
            else
            {
                throw new InvalidOperationException($"The output file {fileName} not written, because the data is null or empty.");
            }

            return docHtml;
        }

        public void Dispose()
        {
            Writer.Dispose();
        }
    }
}
