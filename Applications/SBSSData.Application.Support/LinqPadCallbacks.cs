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
                    string title = $"{league} Game";
                    header = $"Game Results Data for the {numGames.NumDesc(title)}";
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

        public static Func<TableNode, string> ExtendedDsInfo(string? headerCssStyle = null, object? value = null)
        {
            return (t) =>
            {
                string header = "Data Store Information";
                HtmlNode tableHtmlNode = t.TableHtmlNode;
                int depth = t.Depth();
                int index = t.Index();
                if (!string.IsNullOrWhiteSpace(headerCssStyle))
                {
                    tableHtmlNode.SelectSingleNode("./thead/tr/td").Attributes.Add("style", headerCssStyle);
                }

                return header;
            };
        }

        public static Func<TableNode, string> ExtendedGamesTeamPlayers(string? info = null, object? value = null)
        {
            return (t) =>
            {
                string fullLeagueName = info ?? "league";
                string shortLeagueName = fullLeagueName.Substring(0, fullLeagueName.LastIndexOf(' '));
                HtmlNode tableHtmlNode = t.TableHtmlNode;
                string header = t.Header().InnerText;
                int depth = t.Depth();
                int index = t.Index();
                int numEntries = tableHtmlNode.SelectNodes("./tbody/tr").Count();
                HtmlNode? group = null;
                if (depth == 1)
                {
                    group = tableHtmlNode.ParentNode?.PreviousSibling;
                }
                else if (depth == 2)
                {
                    group = t.Parent?.TableHtmlNode.ParentNode.PreviousSibling;
                }

                string groupName = (group != null) ? group.InnerText.Replace("&nbsp;", " ") : string.Empty;

                Console.WriteLine($"Processing {t.Id()} in group {groupName} at depth {t.Depth()} and index {t.Index()}");


                switch (depth)
                {
                    case 0:
                    {
                        header = $"Games, Teams and Players for the {fullLeagueName} League";
                        tableHtmlNode.SelectNodes("./tbody/tr/th").ToList().ForEach(th => th.Attributes.Add("style", "display:none"));
                        tableHtmlNode.SelectSingleNode("./thead/tr/td").Attributes.Add("style", "font-size:1.25em;  background-color:#d62929");
                        //ableHtmlNode.SelectSingleNode("./thead/tr/td/a").Attributes.Add("style", "color:rgb(178, 34, 34, 1)");
                        break;
                    }
                    case 1:
                    {
                        switch (index)
                        {
                            case 0:
                            {
                                //int numGames = tableHtmlNode.SelectNodes("./tbody/tr").Count;
                                string title = $"{shortLeagueName} Game";
                                header = $"Game Results Data for the {numEntries.NumDesc(title)}";
                                break;
                            }
                            case 1:
                            {
                                header = $"Team Summary Data for the {numEntries} {shortLeagueName} Teams";
                                break;
                            }
                            case 2:
                            {
                                header = $"Player Summary Stats for the {numEntries} Players from All Teams in the {shortLeagueName} League";
                                for (int i = 4; i < 7; i++)
                                {
                                    Utilities.AlterTableColumnHeader(tableHtmlNode, i, $"{i - 3}B");
                                }
                                HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
                                lastRow.Attributes.Add("style", "background-color:#ddd; font-weight:500;");
                                break;
                            }
                        }
                        break;
                    }
                    case 2:
                    {
                        //string groupName = t.Parent.TableHtmlNode.ParentNode.PreviousSibling.InnerText;
                        //HtmlNode headerNode = tableHtmlNode.Ancestors("table").First().ParentNode.ParentNode.SelectSingleNode("./th").ParentNode;
                        //if (headerNode.XPath.EndsWith("tr[2]"))
                        if (groupName == "Team Players")
                        {
                            int rowNumber = index + 1;
                            string? teamName = t.Parent?.TableHtmlNode?.SelectSingleNode($"./tbody/tr[{rowNumber}]").FirstChild.InnerText;
                            header = $"Summary Stats for all {ToSpanItalic(teamName)} Players";

                            HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
                            lastRow.Attributes.Add("style", "background-color:#ddd; font-weight:500;");
                            //List<HtmlNode> columnHeaders = tableHtmlNode.SelectNodes("./thead/tr[2]/th").ToList();

                            for (int i = 4; i < 7; i++)
                            {
                                //HtmlNode columnHeader = columnHeaders[i];
                                //HtmlNode newHeader = HtmlNode.CreateNode(columnHeader.OuterHtml.Replace(columnHeader.InnerText, $"{i - 3}B"));
                                //columnHeader.ParentNode.ReplaceChild(newHeader, columnHeader);
                                Utilities.AlterTableColumnHeader(tableHtmlNode, i, $"{i - 3}B");
                            }

                            HtmlNode name = lastRow.SelectSingleNode("./td[2]");
                            name.Attributes.Add("style", "background-color:red");
                            HtmlNode newName = HtmlNode.CreateNode($"<td>{name.InnerText} Totals</td>");
                            name.ParentNode.ReplaceChild(newName, name);

                        }
                        else if (groupName == "Games and Teams")
                        {
                            if (int.IsEvenInteger(index))
                            {
                                header = "Scheduled Game Information";
                            }
                            else
                            {
                                string gameName = tableHtmlNode.ParentNode.PreviousSibling.SelectSingleNode("./table/tbody/tr/td").InnerText;
                                //string visitor = tableHtmlNode.SelectSingleNode("./tbody/tr[1]/td").InnerText;
                                //string home = tableHtmlNode.SelectSingleNode("./tbody/tr[2]/td").InnerText;
                                header = $"Teams and Players for the {ToSpanItalic(gameName)} Game";

                                Utilities.AlterTableColumnHeader(tableHtmlNode, 1, "RS");
                                Utilities.AlterTableColumnHeader(tableHtmlNode, 2, "RA");

                                HtmlNode outcomeCell = tableHtmlNode.SelectSingleNode("./tbody/tr[1]/td[5]");
                                HtmlNode centered = HtmlNode.CreateNode($"<td style=\"text-align:center;\">{outcomeCell.InnerHtml}</td>");
                                outcomeCell.ParentNode.ReplaceChild(centered, outcomeCell);

                                outcomeCell = tableHtmlNode.SelectSingleNode("./tbody/tr[2]/td[5]");
                                centered = HtmlNode.CreateNode($"<td style=\"text-align:center;\">{outcomeCell.InnerHtml}</td>");
                                outcomeCell.ParentNode.ReplaceChild(centered, outcomeCell);
                            }
                        }
                        break;
                    }
                    case 3:
                    {
                        int rowNumber = t.Index() + 1;
                        string teamName = t.Parent?.TableHtmlNode.SelectSingleNode($"./tbody/tr[{rowNumber}]/td").InnerText ?? "Unknown";
                        header = $"Players for {ToSpanItalic(teamName)}";
                        tableHtmlNode = t.TableHtmlNode;

                        HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
                        lastRow.Attributes.Add("style", "background-color:#ddd; font-weight:500;");

                        //List<HtmlNode> columnHeaders = tableHtmlNode.SelectNodes("./thead/tr[2]/th").ToList();

                        for (int i = 4; i < 7; i++)
                        {
                            //HtmlNode columnHeader = columnHeaders[i];
                            //HtmlNode newHeader = HtmlNode.CreateNode(columnHeader.OuterHtml.Replace(columnHeader.InnerText, $"{i - 3}B"));
                            //columnHeader.ParentNode.ReplaceChild(newHeader, columnHeader);
                            Utilities.AlterTableColumnHeader(tableHtmlNode, i, $"{i - 3}B");
                        }

                        HtmlNode name = lastRow.SelectSingleNode("./td[2]");
                        //name.Attributes.Add("style", "background-color:red");
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

                //header += $" &mdash; [Depth:{t.Depth()}; Index:{t.Index()}; Entries:{numEntries}]";
                return header;
            };
        }


        private static string ToSpanItalic(string text)
        {
            return $"""<span style="font-style:italic">{text}</span>""";
        }
    }
}
