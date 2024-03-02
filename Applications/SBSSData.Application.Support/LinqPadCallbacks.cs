﻿// Ignore Spelling: Linq

using HtmlAgilityPack;

namespace SBSSData.Application.Support
{
    /// <summary>
    /// Callbacks used from LINQPad generated HTML to modify the code generated by LINQPad.
    /// </summary>
    public static class LinqPadCallbacks
    {

        public static Func<TableNode, string> DsInfoCallback = (t) => $"Data Store Information"; // &mdash; [Table ID={t.Id()}; Depth,Index=({t.Depth()},{t.Index()})]";
        public static Func<TableNode, string> GamesCallback = (t) =>
        {
            string header = string.Empty;
            HtmlNode tableHtmlNode = t.TableHtmlNode;
            //Console.WriteLine($"In gamesCallback, for table ID [Depth, Index] = {tableHtmlNode.Id} [{t.Depth()}, {t.Index()}]");
            switch (t.Depth())
            {
                case 0:
                {
                    int numGames = tableHtmlNode.SelectNodes("./tbody/tr").Count;
                    HtmlNode gameInfo = tableHtmlNode.SelectSingleNode("./tbody//table/tbody");
                    string league = gameInfo.SelectNodes("./tr")[4].SelectSingleNode("./td").InnerText;
                    header = $"Game Results Data for the {league} Games";
                    break;
                }
                case 1:
                {
                    if (int.IsEvenInteger(t.Index()))
                    {
                        header = "Scheduled Game Information";
                    }
                    else
                    {
                        string visitor = tableHtmlNode.SelectSingleNode("./tbody/tr[1]/td").InnerText;
                        string home = tableHtmlNode.SelectSingleNode("./tbody/tr[2]/td").InnerText;
                        header = $"Teams and Players for the {visitor} vs {home} Game";

                        Utilities.AlterTableColumnHeader(tableHtmlNode, 1, "RS");
                        Utilities.AlterTableColumnHeader(tableHtmlNode, 2, "RA");

                        HtmlNode outcomeCell = tableHtmlNode.SelectSingleNode("./tbody/tr[1]/td[5]");
                        HtmlNode centered = HtmlNode.CreateNode($"<td style=\"text-align:center;\">{outcomeCell.InnerHtml}</td>");
                        outcomeCell.ParentNode.ReplaceChild(centered, outcomeCell);

                        outcomeCell = tableHtmlNode.SelectSingleNode("./tbody/tr[2]/td[5]");
                        centered = HtmlNode.CreateNode($"<td style=\"text-align:center;\">{outcomeCell.InnerHtml}</td>");
                        outcomeCell.ParentNode.ReplaceChild(centered, outcomeCell);
                    }
                    break;
                }
                case 2:
                {
                    int rowNumber = t.Index() + 1;
                    string teamName = t.Parent?.TableHtmlNode.SelectSingleNode($"./tbody/tr[{rowNumber}]/td").InnerText ?? "Unknown";
                    header = $"Players for {teamName}";
                    tableHtmlNode = t.TableHtmlNode;

                    HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
                    lastRow.Attributes.Add("style", "background-color:#ddd; font-weight:500;");

                    List<HtmlNode> columnHeaders = tableHtmlNode.SelectNodes("./thead/tr[2]/th").ToList();

                    for (int i = 4; i < 7; i++)
                    {
                        HtmlNode columnHeader = columnHeaders[i];
                        HtmlNode newHeader = HtmlNode.CreateNode(columnHeader.OuterHtml.Replace(columnHeader.InnerText, $"{i - 3}B"));
                        columnHeader.ParentNode.ReplaceChild(newHeader, columnHeader);
                    }

                    HtmlNode name = lastRow.SelectSingleNode("./td[2]");
                    name.Attributes.Add("style", "background-color:red");
                    HtmlNode newName = HtmlNode.CreateNode($"<td>{name.InnerText} Totals</td>");
                    name.ParentNode.ReplaceChild(newName, name);

                    // Remove the number of games column
                    tableHtmlNode.SelectSingleNode("thead/tr[2]/th").Remove();
                    tableHtmlNode.SelectNodes("tbody/tr/td[1]").ToList().ForEach(n => n.Remove());

                    break;
                }

                default:
                {
                    break;
                }
            }

            return header;
        };

    }
}
