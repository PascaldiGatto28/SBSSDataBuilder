// Ignore Spelling: Linq Css

using System.Reflection;

using HtmlAgilityPack;

using LINQPad;

using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using Query = SBSSData.Softball.Stats.Query;

namespace SBSSData.Application.LinqPadQuerySupport
{
    public class GamesTeamPlayersV3 : IHtmlCreator
    {
        private static HeadElement[] headElements =
        {
            new HeadElement("meta", [["name", "author"], ["content", "Richard Levaro"]]),
            new HeadElement("meta", [["data", "description"], ["content", "All data for games, teams and players"]]),
            new HeadElement("meta", [["name", "viewport"], ["content", "width=device-width, initial-scale=1.0"]]),
            new HeadElement("meta", [["http-equiv", "cache-control"], ["content", "no-cache"]]),
            new HeadElement("title", [["Games, Teams & Players", ""]]),
            new HeadElement("link", [["rel", "shortcut icon"], ["type", "image/x-icon"], ["href", "../SBSSData.ico"]])
        };

        public GamesTeamPlayersV3()
        {
            Values = [];
            ResourceName = $"{this.GetType().Name}.html";
        }

        public List<object> Values
        {
            get;
            set;
        }

        public string ResourceName
        {
            get;
            set;
        }

        public string BuildHtmlPage(string seasonText, string dataStoreFolder, Action<object>? callback = null)
        {
            Action<object>? actionCallback = callback; // == null ? (v) => Console.WriteLine(v.ToString()) : callback;
            string season = seasonText.RemoveWhiteSpace();

            string changedHtml = string.Empty;

            Assembly assembly = typeof(GamesTeamPlayersV3).Assembly;
            string resName = assembly.FormatResourceName(ResourceName);
            byte[] bytes = assembly.GetEmbeddedResourceAsBytes(resName);
            string html = bytes.ByteArrayToString();
            html = html.Replace("[[Season YYYY]]", Utilities.SwapSeasonText(seasonText));

            string path = $"{dataStoreFolder}{season}LeaguesData.json";
            using (DataStoreContainer dsContainer = DataStoreContainer.Instance(path))
            {

                Query query = new Query(dsContainer);
                //DataStoreInformation dsInfo = new DataStoreInformation(dsContainer);
                //string dsInfoHeaderStyle = "background-color:#d62929;";
                string gtpHeaderStyle = "background-color:#d62929; width:642px;";

                IEnumerable<Game> playedGames = query.GetPlayedGames();
                var leagueNames = query.GetLeagueDescriptions().OrderBy(d => d.LeagueCategory).Select(l => new
                {
                    Day = l.LeagueDay,
                    Category = l.LeagueCategory,
                    FullLeagueName = l.ToString(),
                    ShortLeagueNume = $"{l.LeagueDay} {l.LeagueCategory}"
                });

                using (HtmlGenerator generator = new HtmlGenerator())
                {
                    string seasonDiv = $"""
                                       <div id="seasonText">
                                            {seasonText}  
                                       </div>
                                       """;

                    string expandCollapseHtml = """
                                                <div>
                                                     <button class="sbss" onclick = "viewAll(true)">Expand All Tables</button>
                                                     <button class="sbss" onclick = "viewAll(false)">Collapse All Tables</button>
                                                </div> 
                                                """;
                    //generator.WriteRawHtml(seasonDiv);

                    //string displayRankingColumn = """
                    //                               <script type=text/javascript>
                    //                                   function setColumn (checked)
                    //                                   {
                    //                                        alert(checked);
                    //                                   }
                    //                               </script>  
                    //                               <div style="text-align:right;">
                    //                                   <input type="checkbox" id="rankingColumn" name="ranking" value="true" onchange="setColumn(this.checked);">
                    //                                   <label for="ranking">Hide the ranking column</label>
                    //                               </div>
                    //                               """;

                    generator.WriteRawHtml(expandCollapseHtml);

                    //actionCallback("expandCollapseHtml raw Html written");

                    //generator.WriteRootTable(dsInfo, Utilities.ExtendedDsInfo(dsInfoHeaderStyle));
                    //actionCallback("dsInfo root table written");

                    foreach (var leagueName in leagueNames)
                    {
                        IEnumerable<Game> leagueGames = playedGames.Where(g => (g.GameInformation.LeagueDay == leagueName.Day) &&
                                                                               (g.GameInformation.LeagueCategory == leagueName.Category));

                        if (leagueGames.Any())
                        {
                            var games = leagueGames.Select(g => new
                            {
                                Games = new GameInformationDisplay(g.GameInformation),
                                Teams = g.Teams.Select(t => new TeamStatsDisplay(new TeamStats(t))),
                            });

                            string fullLeagueName = leagueName.FullLeagueName;
                            string shortLeagueName = $"{leagueName.Day} {leagueName.Category}";

                            IEnumerable<TeamSummaryStatsDisplay> tss = query.GetTeamsPlayersStats(leagueName.Category, leagueName.Day)
                                                                            .Select(t => new TeamSummaryStatsDisplay(t));
                            IEnumerable<PlayerStatsRank> playerStatsRanks = query.GetLeaguePlayerStatsRank(leagueName.Category, leagueName.Day);//.Dump();
                            IEnumerable<PlayerStatsRankDisplay> playerStatRankDisplay =
                                             playerStatsRanks.Select(psr => new PlayerStatsRankDisplay(psr.Player, psr.PlayerRank));

                            var gtp = new
                            {
                                GamesAndTeams = games,
                                TeamPlayers = tss,
                                //HideRank = Util.RawHtml(displayRankingColumn),
                                Players = playerStatRankDisplay
                            };

                            generator.WriteRootTable(gtp, ExtendedGamesTeamPlayers($"{fullLeagueName}", gtpHeaderStyle));
                        }

                    }

                    if (actionCallback != null)
                    {
                        actionCallback($"{this.GetType().Name} HTML page created.");
                    }

                    string htmlNode = html.Substring("<div class=\"IntroContent\"", "</body", true, false);
                    HtmlNode title = HtmlNode.CreateNode(htmlNode);
                    changedHtml = generator.DumpHtml(pageTitle: title,
                                                     //cssStyles: StaticConstants.LocalStyles,
                                                     //javaScript: StaticConstants.LocalJavascript,
                                                     cssStyles: StaticConstants.LocalStyles + StaticConstants.HelpStyles,
                                                     javaScript: StaticConstants.LocalJavascript + StaticConstants.HelpJavascript,
                                                     collapseTo: 1,
                                                     headElements: headElements.ToList());
                }

            }

            return changedHtml;
        }

        #region Callback Code
        public static string ProcessPlayerSummaryStats(HtmlNode tableHtmlNode, int numEntries, string shortLeagueName)
        {
            string imagePath = "../PlayerPhotos/";
            string header = string.Empty;

            Utilities.UpdatePlayerColumnNames(tableHtmlNode);
            Utilities.UpdatePlayerHeaderTitles(tableHtmlNode, true, true);

            HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
            lastRow.Attributes.Add("style", "background-color:#ddd; font-weight:500;");

            HtmlNode root = tableHtmlNode.Ancestors("#document").First();
            IEnumerable<HtmlNode> rows = tableHtmlNode.SelectNodes($"./tbody/tr");

            List<HtmlNode> rankingTables = tableHtmlNode.SelectNodes("./tbody//tr/td[last()]/table").ToList();
            rankingTables.ForEach(rt =>
            {
                rt.SetAttributeValue("style", "width:160px");
                string playerName = rt.ParentNode.ParentNode.SelectSingleNode("./td[2]").InnerText;
                string playerFirstName = playerName.Substring(0, playerName.IndexOf(' '));
                string[] headers = [
                                      $"{playerFirstName}'s position in the list of all players AVG values",
                                      $"{playerFirstName}'s position in the list of all players SLG values",
                                      $"{playerFirstName}'s position in the list of all players OBP values",
                                      $"{playerFirstName}'s position in the list of all players OPS values",
                                   ];
                List<HtmlNode> rankingTableHeaders = Utilities.GetTableColumnHeaders(rt);
                foreach (HtmlNode th in rankingTableHeaders)
                {
                    th.SetAttributeValue("title", headers[rankingTableHeaders.IndexOf(th)]);
                }
            });

            HtmlNode overlay = HtmlNode.CreateNode(StaticConstants.OverlayTemplate);
            //string overlayHtml = overlay.OuterHtml;
            HtmlNode body = root.SelectSingleNode("//body");
            HtmlNode introContent = root.SelectSingleNode("//div[@id='IntroContent']");
            HtmlNode overlayContainer = HtmlNode.CreateNode($"""<div id="{shortLeagueName.RemoveWhiteSpace()}OverlayContainer"></div>""");
            body.InsertAfter(overlayContainer, body.FirstChild);
            int numberQualified = 0;
            foreach (HtmlNode row in rows)
            {
                HtmlNode player = row.SelectSingleNode("./td[2]");
                player.Attributes.Add("style", "cursor:pointer;");
                string playerName = player.InnerText;

                // The rank table needs to be "copied" to the overlay, but the ID needs to be changed
                // so collapse and expand works, the title should be set to the value of the 
                // source rank table. So we get those values, and then restore them after the source
                // rank has been copied.
                HtmlNode rankTable = row.SelectSingleNode("./td[last()]/table");
                rankTable.Attributes.Add("style", "width:160px");
                string rankTableId = rankTable.Id;
                TableNode rankTableNode = new TableNode(rankTable);
                string headerText = rankTable.SelectSingleNode("./tbody/tr[1]/td").InnerText;
                string rankTableHeader = string.Empty;
                if (headerText == "NA")
                {
                    rankTableHeader = "Not Enough Data";
                }
                else
                {
                    rankTableHeader = "Player Rankings";
                    numberQualified++;
                }

                if (playerName.Contains("Totals"))
                {
                    rankTableHeader = "No Rankings for Totals";

                    // The summary table is not a qualified PLAYER
                    numberQualified = Math.Max(numberQualified--, 0);
                }

                header = $"Player Summary Stats for the {numEntries} Players";
                if (numberQualified > 0)
                {
                    header += $" and Rankings for {numberQualified} Players With Enough Plate Appearances for All Teams";
                }

                // Set the new values and then restore the previous values
                rankTable.Id = rankTable.Id + playerName.RemoveWhiteSpace();

                // The players who qualify for ranking
                TableTree.SetTableHeader(rankTableNode, (t) => rankTableHeader);
                string rankTableHtml = rankTable.OuterHtml;
                rankTable.Id = rankTableId;

                // Finish up the overlay for this player. 

                Dictionary<string, string> map = PlayerPhotos.GetPlayerName2ImageNameMap();

                string playerKey = string.Empty;
                string? imageName;
                string[] playerNameSplit = playerName.Split(' ');
                if (playerName.Contains("Totals") && (playerNameSplit.Length > 2))
                {
                    playerKey = "Totals, League";
                    imageName = "League_Totals";
                }
                else
                {
                    if (playerNameSplit.Length == 2)
                    {
                        playerKey = $"{playerNameSplit[1]}, {playerNameSplit[0]}";
                    }
                    else
                    {
                        playerKey = $"Unknown, {string.Empty}";
                    }

                    if (!map.TryGetValue(playerKey, out imageName))
                    {
                        imageName = "Available_Photo-Not";
                    }
                }

                string newOverlayHtml = StaticConstants.BuildOverlay(imagePath, imageName, rankTableHtml);
                HtmlNode newOverlay = HtmlNode.CreateNode(newOverlayHtml);

                newOverlay.Id = $"{playerName.RemoveWhiteSpace()}{shortLeagueName.RemoveWhiteSpace()}Overlay";
                string newOverlayId = newOverlay.Id;
                string dataName = $"{playerName.RemoveWhiteSpace()}";
                newOverlay.Attributes.Add("dataName", dataName);
                newOverlay.Attributes.Add("style", "display:none");
                newOverlayHtml = newOverlay.OuterHtml.Replace("\r\n", " ");

                // Insert this new overlay in the page so it can be found by the mouseover and mouseout event handlers.

                //HtmlNode overlayContainer = HtmlNode.CreateNode("""<div style="display:none" id="overlayContainer"></div>""");
                //body.InsertAfter(overlayContainer, body.FirstChild);
                overlayContainer.AppendChild(newOverlay);
                //body.InsertAfter(newOverlay, body.Child);
                //body.InsertAfter(newOverlay, body.FirstChild);
                player.Attributes.Add("onmouseover", $"getElementById('{newOverlayId}').style.display='table-cell'");
                player.Attributes.Add("onmouseout", $"getElementById('{newOverlayId}').style.display='none';");

                //tableHtmlNode.SelectNodes("./tbody//tr/td[last()]").ToList().ForEach(n =>
                //{
                //    n.Attributes.Add("style", "width:160px; display:none");
                //});

                //tableHtmlNode.SelectSingleNode("./thead/tr[2]/th[last()]").Attributes.Add("style", "display:none");
            }

            return header;
        }

        public static Func<TableNode, string> ExtendedGamesTeamPlayers(string? info = null, string? headerCssStyle = null, object? value = null)
        {
            return (t) =>
            {
                string fullLeagueName = info ?? "league";
                string shortLeagueName = fullLeagueName.Substring(0, fullLeagueName.LastIndexOf(' '));
                HtmlNode tableHtmlNode = t.TableHtmlNode;
                string header = t.Header().InnerText;
                int depth = t.Depth();
                int index = t.Index();

                // Do not count the summary totals row. (see below)
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

                //Console.WriteLine($"Processing {t.Id()} in group {groupName} at depth {t.Depth()} and index {t.Index()}");


                switch (depth)
                {
                    case 0:
                    {
                        header = $"Games, Teams and Players for the {fullLeagueName} League";
                        tableHtmlNode.SelectNodes("./tbody/tr/th").ToList().ForEach(th => th.Attributes.Add("style", "display:none"));
                        tableHtmlNode.SelectSingleNode("./thead/tr/td").Attributes.Add("style", headerCssStyle);

                        //HtmlNode headerNode = t.Header();
                        //HtmlNode helpNode = HtmlNode.CreateNode(helpNodeHtml);
                        //headerNode.ParentNode.AppendChild(helpNode);
                        break;
                    }
                    case 1:
                    {
                        //HtmlNode headerNode = t.Header();
                        //HtmlNode helpNode = HtmlNode.CreateNode(helpNodeHtml);
                        //headerNode.ParentNode.AppendChild(helpNode);
                        switch (index)
                        {
                            case 0:
                            {
                                string title = $"{shortLeagueName} Game";
                                if (numEntries == 0)
                                {
                                    header = "No Games Have Been Played";
                                }
                                else
                                {
                                    header = $"Game Results Data for the {numEntries.NumDesc(title)}";
                                    List<HtmlNode> tableColumnHeaders = Utilities.GetTableColumnHeaders(tableHtmlNode);
                                    tableColumnHeaders[0].SetAttributeValue("title", "Game information for each scheduled game played");
                                    tableColumnHeaders[1].SetAttributeValue("title", "Teams and the players and their stats for each game");
                                }
                                break;
                            }
                            case 1:
                            {
                                if (numEntries == 0)
                                {
                                    header = "No Teams Have Played";
                                }
                                else
                                {
                                    header = $"Team Summary Data and Standings for the {numEntries} {shortLeagueName} Teams";
                                    List<HtmlNode> tableColumnHeaders = Utilities.GetTableColumnHeaders(tableHtmlNode);
                                    tableColumnHeaders[0].SetAttributeValue("title", "Team Name");
                                    tableColumnHeaders[1].SetAttributeValue("title", "Games played");
                                    tableColumnHeaders[2].SetAttributeValue("title", "Games won");
                                    tableColumnHeaders[3].SetAttributeValue("title", "Games lost");
                                    tableColumnHeaders[4].SetAttributeValue("title", "Runs scored by this team");
                                    tableColumnHeaders[5].SetAttributeValue("title", "Runs scored by the opposing teams");
                                    tableColumnHeaders[6].SetAttributeValue("title", "Total number of hits");
                                    tableColumnHeaders[7].SetAttributeValue("title", "Player summary stats for this team");
                                }
                                break;
                            }
                            case 2:
                            {
                                if (numEntries == 0)
                                {
                                    header = "No Players Have Played";
                                }
                                else
                                {
                                    header = ProcessPlayerSummaryStats(tableHtmlNode, numEntries - 1, shortLeagueName);
                                }
                                break;
                            }
                        }
                        break;
                    }
                    case 2:
                    {
                        if (groupName == "Team Players")
                        {
                            int rowNumber = index + 1;
                            string? teamName = t.Parent?.TableHtmlNode?.SelectSingleNode($"./tbody/tr[{rowNumber}]").FirstChild.InnerText;
                            header = $"Summary Stats for all {Utilities.ToSpanItalic(teamName ?? string.Empty)} Players";

                            HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
                            lastRow.Attributes.Add("style", "background-color:#ddd; font-weight:500;");

                            Utilities.UpdatePlayerColumnNames(tableHtmlNode);
                            Utilities.UpdatePlayerHeaderTitles(tableHtmlNode, true);

                            HtmlNode name = lastRow.SelectSingleNode("./td[2]");
                            HtmlNode newName = HtmlNode.CreateNode($"<td>{name.InnerText} Totals</td>");
                            name.ParentNode.ReplaceChild(newName, name);

                        }
                        else if (groupName == "Games and Teams")
                        {
                            if (int.IsEvenInteger(index))
                            {
                                header = "Scheduled Game Information";
                                List<HtmlNode> columnHeaders = Utilities.GetTableColumnHeaders(tableHtmlNode);
                                columnHeaders[0].SetAttributeValue("title", "Game title, the visiting team is first");
                                columnHeaders[1].SetAttributeValue("title", "A unique ID for the scheduled game");
                                columnHeaders[2].SetAttributeValue("title", "The original start date and time for the game");
                                columnHeaders[3].SetAttributeValue("title", "The league in which this game is played");
                            }
                            else
                            {
                                string gameName = tableHtmlNode.ParentNode?.PreviousSibling.SelectSingleNode("./table/tbody/tr/td").InnerText ?? "Unknown";
                                header = $"Teams and Players for the {Utilities.ToSpanItalic(gameName)} Game";

                                Utilities.AlterTableColumnHeader(tableHtmlNode, 2, "RS");
                                Utilities.AlterTableColumnHeader(tableHtmlNode, 3, "RA");

                                HtmlNode outcomeCell = tableHtmlNode.SelectSingleNode("./tbody/tr[1]/td[5]");
                                HtmlNode centered = HtmlNode.CreateNode($"<td style=\"text-align:center;\">{outcomeCell.InnerHtml}</td>");
                                outcomeCell.ParentNode.ReplaceChild(centered, outcomeCell);

                                outcomeCell = tableHtmlNode.SelectSingleNode("./tbody/tr[2]/td[5]");
                                centered = HtmlNode.CreateNode($"<td style=\"text-align:center;\">{outcomeCell.InnerHtml}</td>");
                                outcomeCell.ParentNode.ReplaceChild(centered, outcomeCell);

                                List<HtmlNode> columnHeaders = Utilities.GetTableColumnHeaders(tableHtmlNode);
                                columnHeaders[0].SetAttributeValue("title", "Team Name");
                                columnHeaders[1].SetAttributeValue("title", "Runs scored by this team");
                                columnHeaders[2].SetAttributeValue("title", "Runs scored by the opposing team");
                                columnHeaders[3].SetAttributeValue("title", "Number of hits for this game");
                                columnHeaders[4].SetAttributeValue("title", "\"Loss\" if the game is lost; \"Win\" if won");
                                columnHeaders[5].SetAttributeValue("title", "Player stats for this team and this game");
                            }
                        }
                        break;
                    }
                    case 3:
                    {
                        int rowNumber = t.Index() + 1;
                        string teamName = t.Parent?.TableHtmlNode.SelectSingleNode($"./tbody/tr[{rowNumber}]/td").InnerText ?? "Unknown";
                        header = $"Players for {Utilities.ToSpanItalic(teamName)}";
                        tableHtmlNode = t.TableHtmlNode;

                        HtmlNode lastRow = tableHtmlNode.SelectSingleNode("./tbody/tr[last()]");
                        lastRow.Attributes.Add("style", "background-color:#ddd; font-weight:500;");

                        Utilities.UpdatePlayerColumnNames(tableHtmlNode);

                        HtmlNode name = lastRow.SelectSingleNode("./td[2]");
                        HtmlNode newName = HtmlNode.CreateNode($"<td>{name.InnerText} Totals</td>");
                        name.ParentNode.ReplaceChild(newName, name);

                        // Remove the number of games column
                        tableHtmlNode.SelectSingleNode("thead/tr[2]/th").Remove();
                        tableHtmlNode.SelectNodes("tbody/tr/td[1]").ToList().ForEach(n => n.Remove());

                        Utilities.UpdatePlayerHeaderTitles(tableHtmlNode, false);
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
        #endregion Callback code
    }
}