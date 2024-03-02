using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Samples
{
    public class Program
    {
        private static void Main()
        {
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
    }
}
