using System.Diagnostics;

using LINQPad;

using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

using WinSCP;

using Query = SBSSData.Softball.Stats.Query;

namespace SBSSData.Application.Samples
{
    public class Program
    {
        private static void Main()
        {
            //LogSessions(true);
            //GamesTeamPlayersV3(true);
            //WinSCPSync();
            PlayerSheets();
            //RunUtilLP();
            //RunLINQPad();
            //SimpleTestHtml();
            //TestTableCode();
            //L2HTML();
            //GTP();
            //CheckHtml();

        }

        public static void PlayerSheets(bool displayHtml = false, bool useCallback = false)
        {
            string seasonText = "2024 Winter";
            string dsFolder = $@"J:\SBSSDataStore\";

            PlayerSheets playerSheets = new PlayerSheets();
            string html = playerSheets.BuildHtmlPage(seasonText, dsFolder, callback);

            //string folderName = @"d:\Temp\"; //@$"{dsFolder}Html Data\";
            //string fileName = "PlayerSheetsContainer.html";
            //string htmlFilePath = @"D:\Users\Richard\Documents\Visual Studio 2022\Github Projects\SBSS\SBSSDataBuilder\Applications\SBSSData.Application.LinqPadQuerySupport\PlayerSheetsContainer.html";
            //HtmlDocument htmlDoc = new HtmlDocument();
            //htmlDoc = PageContentUtilities.GetPageHtmlDocument(htmlFilePath);
            //HtmlNode root = htmlDoc.DocumentNode;
            //HtmlNode sheets = root.SelectSingleNode("//iframe[@id='sheets']");
            //sheets.Attributes["srcDoc"].Remove();
            //sheets.Attributes.Add("srcDoc", html);

            //List<string> optionValues = query.GetPlayerNames().Select(p => $"""<div name="{map[p]}">{p.BuildDisplayName()}</div>""").ToList();

            //string html = optionValues.ToString<string>("\r\n");
            //string playerSheetsContainer = File.ReadAllText(htmlFilePath);
            //playerSheetsContainer

            string folderName = @"d:\Temp\"; //@$"{dsFolder}Html Data\";
            string fileName = "xx.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe", $"\"{htmlFilePath}\"");
        }



        public static void LogSessions(bool displayHtml = false, bool useCallback = false)
        {
            string seasonText = "2024 Winter";
            string dsFolder = $@"J:\SBSSDataStore\";

            LogSessions ls2Html = new LogSessions();
            string html = ls2Html.BuildHtmlPage(seasonText, dsFolder, callback);

            string folderName = @$"{dsFolder}Html Data\";
            string fileName = "LogSessions.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            if (displayHtml)
            {
                Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe", $"\"{htmlFilePath}\"");
            }

            Console.WriteLine("LogSessions completed");
        }

        public static Action<object> callback = (v) => v.Dump("What the Flock!");

        public static void GamesTeamPlayersV3(bool displayHtml = false, bool useCallback = false)
        {
            string seasonText = "2024 Winter";
            string dsFolder = $@"J:\SBSSDataStore\";

            GamesTeamPlayersV3 gtp = new GamesTeamPlayersV3();
            string html = gtp.BuildHtmlPage(seasonText, dsFolder, useCallback ? callback : null);

            string folderName = @$"{dsFolder}Html Data\";
            string fileName = "GamesTeamPlayersV3.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            if (displayHtml)
            {
                Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe", $"\"{htmlFilePath}\"");
            }

            Console.WriteLine("GamesTeamPlayersV3 completed");
        }

        public static CheckQueryResults<IEnumerable<Player>> ReportLeaguePlayers(Query queries, string leagueCategory, string day, HtmlGenerator generator)
        {
            IEnumerable<Player> players = queries.GetLeaguePlayers(leagueCategory, day);
            string info = $"Player stats for {day} {leagueCategory} Fall 2023";
            generator.Write(players);
            var leaguePlayers = CheckQueryResults<IEnumerable<Player>>.CheckResults(players);
            generator.Write(leaguePlayers);
            return leaguePlayers;
        }

        public static void WinSCPSync()
        {
            // Set up session options
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Ftp,
                HostName = "ftp.walkingtree.com",
                UserName = "quietcre",
                Password = "85232WindingWay",
            };

            using (Session session = new Session())
            {
                //session.FileTransferred = (s,e) => e.Dump();
                // Connect
                session.Open(sessionOptions);

                var comparison = session.CompareDirectories(SynchronizationMode.Local, @"J:\SBSSDataStore\Html Data", "/quietcre/Data", true).Dump();

                string isError = string.Empty;
                try
                {
                    // Synchronize files
                    SynchronizationResult synchronizationResult;
                    synchronizationResult =
                        session.SynchronizeDirectories(
                            SynchronizationMode.Remote,
                            @"J:\SBSSDataStore\Html Data",
                            "/quietcre/Data", false).Dump();

                    synchronizationResult.Check();
                    Console.WriteLine($"Failures {synchronizationResult.Failures}");
                    Console.WriteLine($"Uploads {synchronizationResult.Uploads}");
                    Console.WriteLine($"Downloads {synchronizationResult.Downloads}");
                }
                catch (Exception exception)
                {
                    isError = $" with error {exception.Message}";
                    Console.WriteLine($"Error: {exception}");
                }

                Console.WriteLine($"WinSCP synchronization completed{isError}");
            }
        }

        public static void RunLP()
        {
            string linqPad = @"C:\Program Files\LINQPad8\LINQPad8.exe";
            string query = @"D:\Users\Richard\OneDrive\RBL Personal\Development\LINQPad\LINQPad Queries\SBSSDataBuilder\DailyCompositeRun.linq -run";
            //string query = @"D:\Users\Richard\OneDrive\RBL Personal\Development\LINQPad\LINQPad Queries\Working Temp\CheckingArgs.linq -run";
            using (_ = System.Diagnostics.Process.Start(linqPad, query))
            {
                Thread.Sleep(5000);
                Process.Start("taskkill", "/F /IM LinqPad8*");
            }
        }

        public static void RunUtilLP()
        {
            string query = @"D:\Users\Richard\OneDrive\RBL Personal\Development\LINQPad\LINQPad Queries\Working Temp\CheckingArgs.linq";
            string[] args = ["Help", "Me", "Please"];
            Console.WriteLine(args.ToString<string>("\r\n"));
            //Util.Run(query, QueryResultFormat.HtmlFragment);
        }


        public static void RunLINQPad()
        {
            // This fails because it is using the LinqPadRuntime which produces the wrong (no body) HTML
            Util.Run(@"SBSSDataBuilder\Testing LP and VS Table Rendering", QueryResultFormat.HtmlFragment);
        }
        public static void CheckHtml()
        {
            string seasonText = "2024 Winter";
            string season = seasonText.RemoveWhiteSpace();
            string path = $@"J:\SBSSDataStore\{season}LeaguesData.json";
            DataStoreContainer dsContainer = DataStoreContainer.Instance(path);
            DataStoreInformation dsInfo = new DataStoreInformation(dsContainer);
            //dsInfo.Dump();
            Query query = new Query(dsContainer);

            IEnumerable<Player> players = query.GetPlayedGames().Skip(25).First().Teams.First().Players;
            string outputPath = @"D:\temp\junk\vsx.html";
            string html = string.Empty;

            using (HtmlGenerator generator = new HtmlGenerator())
            {
                generator.WriteRootTable(players, PlayersCallback);
                html = generator.DumpHtml();
            }

            File.WriteAllText(outputPath, html);
            Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe", $"{outputPath}");

            string outputBodyPath = outputPath.Replace("vsx.html", "vsxBody.html");
            string body = html.Substring("<body>", "</body>", true, true);
            File.WriteAllText(outputBodyPath, body);
            Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", outputBodyPath);

            dsContainer.Dispose();
        }

        public static Func<TableNode, string> PlayersCallback = (t) =>
        {
            string headerText = "Holy crap!, header is null";
            if (t.Header() != null)
            {
                headerText = t.Header().InnerText;
            }
            Console.WriteLine($"GetActivePlayers header = {headerText}");
            return headerText;
        };

        public static void GTP()
        {
            string seasonText = "2024 Winter";
            string dsFolder = $@"J:\SBSSDataStore\";

            GamesTeamPlayersV3 gtp = new GamesTeamPlayersV3();
            string html = gtp.BuildHtmlPage(seasonText, dsFolder);
            //foreach (object value in gtp.Values)
            //{
            //    value.Dump();
            //}

            string folderName = @"D:\Temp\junk\Testing Table Generation\"; // @$"{ dsFolder}Html Data\";
            string fileName = "vsGamesTeamPlayers.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe", $"\"{htmlFilePath}\"");
        }

        public static void L2HTML()
        {
            string seasonText = "2024 Winter";
            string dsFolder = $@"J:\SBSSDataStore\";

            LogSessions ls2Html = new LogSessions();
            string html = ls2Html.BuildHtmlPage(seasonText, dsFolder, null);

            string folderName = @$"{dsFolder}Html Data\";
            string fileName = "LogSessions.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Beta\Application\msedge.exe", $"\"{htmlFilePath}\"");
        }

        public static void TestTableCode()
        {
            string seasonText = "2024 Winter";
            string season = seasonText.RemoveWhiteSpace();
            string path = $@"J:\SBSSDataStore\{season}LeaguesData.json";
            DataStoreContainer dsContainer = DataStoreContainer.Instance(path);
            DataStoreInformation dsInfo = new DataStoreInformation(dsContainer);
            dsInfo.Dump();

            string bodyHtml = @"<table>
                        <thead>
                        <tr>
                        <th>Hello</th><th>GoodBye</th>
                        </tr>
                        </thead>
                        <tbody>
                        <tr><td>One</td><td>Two</td></tr>
                        <tr><td>Two</td><td>Buckle</td></tr>
                        <tr><td>My</td><td>Shoe</td></tr>
                        <tr><td>Three</td><td>Four</td></tr>
                        <tr><td>Shut</td><td>The Door</td></tr>
                        </tbody>
                        <tfoot>
                        </tfoot>
                        <tr>
                            <td colspan=""2"">Howdy Doody</td>
                        </tr>
                        </table>";

            string html = string.Empty;
            using (HtmlGenerator generator = new HtmlGenerator())
            {
                generator.WriteRawHtml(bodyHtml);
                html = generator.DumpHtml();
            }

            Console.WriteLine(Util.RawHtml(html)); ;
            File.WriteAllText(@"D:\Temp\junk\Testing Table Generation\vstx.html", html);

            dsContainer.Dispose();
        }

        public static void SimpleTestHtml()
        {
            List<SimpleTest> simpleTest = new()
            {
                new SimpleTest("One", "Two"),
                new SimpleTest("Buckle", "My shoe"),
                new SimpleTest("Three", "Four"),
                new SimpleTest("Shut", "The Door"),
                new SimpleTest("Five", "Six"),
                new SimpleTest("Pickup", "Sticks")
            };

            string html = string.Empty;
            using (TextWriter writer = Util.CreateXhtmlWriter())
            {
                writer.Write(simpleTest);
                html = writer.ToString();
            }

            int bodyIndex = html.IndexOf("<body>");

            string table = $"<html>{html.Substring(bodyIndex)}";
            Console.WriteLine(table);

            File.WriteAllText(@"D:\Temp\junk\Testing Table Generation\vsSimpleTest.html", table);
        }
    }

    public record SimpleTest(string Hello, string Goodbye)
    {
    }

}
