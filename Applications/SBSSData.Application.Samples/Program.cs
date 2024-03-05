using System.Diagnostics;

using SBSSData.Application.LinqPadQuerySupport;
using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Samples
{
    public class Program
    {
        private static void Main()
        {
            L2HTML();
            //GTP();
            //CheckHtml();
            /*
            string htmlOutput = @"D:\Users\Richard\AppData\Local\SBSSData-Application-Samples\HtmlOutput";
            string dataStorePath = @"TestData\LeaguesData.json";
            DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath);
            Console.WriteLine(dsContainer.ToString());

            Query query = new(dsContainer);

            //CheckQueryResults query = new CheckQueryResults(dataStorePath);


            using (HtmlGenerator generator = new())
            {
                //generator.Write(dsContainer.DataStore);

                //LeagueDescription ld = query.LeagueDescriptions.First();
                //generator.Write(ld);
                //var ldResults = CheckQueryResults<LeagueDescription>.CheckResults(ld);
                //generator.Write(ldResults);
                //Console.WriteLine(ldResults.ToString());

                //Player player = query.Players.First();
                //generator.Write(player);
                //var playerResults = CheckQueryResults<Player>.CheckResults(player);
                //generator.Write(playerResults);
                //Console.WriteLine(playerResults.ToString());

                //PlayerStats playerStats = new(player)
                //{
                //    NumGames = 67
                //};
                //generator.Write(playerStats);
                //var statsResults = CheckQueryResults<PlayerStats>.CheckResults(playerStats);
                //generator.Write(statsResults);
                //Console.WriteLine(statsResults.ToString());

                //Game game = query.PlayedGames.First();
                //generator.Write(game);
                //var gameResults = CheckQueryResults<Game>.CheckResults(game);
                //generator.Write(gameResults);
                //Console.WriteLine(gameResults.ToString());


                var reportLeaguePlayers = ReportLeaguePlayers(query, "Community", "Friday", generator);
                IEnumerable<Player> players = query.GetLeaguePlayersSummary("Community", "Friday");
                generator.Write(players);

                //generator.DumpHtml("TestResults.html");
                generator.DumpHtml();
                //new string[] { "A Summary of all Friday Community players", "Testing \"Summary of all Friday Community players\"" },
                //new string[] { "Stats for players", "Test Results" });

            }

            dsContainer.Dispose();
            */
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

        //pubic static CheckQueryResults<Player> ReportPlayer(Query query, string playerName HtmlGenerator generator)
        //{
        //    Player player = query.Players.Wh
        //    var playerResults = CheckQueryResults<Player>.CheckResults(player);
        //    generator.Write(playerResults);
        //    Console.WriteLine(playerResults.ToString());
        //}

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
            Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Dev\Application\msedge.exe", $"{outputPath}");

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
            Console.WriteLine($"Players header = {headerText}");
            return headerText;
        };

        public static void GTP()
        {
            string seasonText = "2024 Winter";
            string dsFolder = $@"J:\SBSSDataStore\";

            GamesTeamPlayers gtp = new GamesTeamPlayers();
            string html = gtp.BuildHtmlPage(seasonText, dsFolder);
            //foreach (object value in gtp.Values)
            //{
            //    value.Dump();
            //}

            string folderName = @$"{dsFolder}Html Data\";
            string fileName = "GamesTeamPlayers.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Dev\Application\msedge.exe", $"\"{htmlFilePath}\"");
        }

        public static void L2HTML()
        {
            string seasonText = "2024 Winter";
            string dsFolder = $@"J:\SBSSDataStore\";

            LogSessionsToHtml ls2Html = new LogSessionsToHtml();
            string html = ls2Html.BuildHtmlPage(seasonText, dsFolder, null);

            string folderName = @$"{dsFolder}Html Data\";
            string fileName = "LogSessions.html";
            string htmlFilePath = $"{folderName}{fileName}";

            File.WriteAllText(htmlFilePath, html);
            Process.Start(@"C:\Program Files (x86)\Microsoft\Edge Dev\Application\msedge.exe", $"\"{htmlFilePath}\"");
        }
    }
}
