using SBSSData.Application.Support;
using SBSSData.Softball;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Samples
{
    public class Program
    {
        private static void Main()
        {
            string htmlOutput = @"D:\Users\Richard\AppData\Local\SBSSData-Application-Samples\HtmlOutput";
            string dataStorePath = @"TestData\LeaguesData.json";
            DataStoreContainer dsContainer = DataStoreContainer.Instance(dataStorePath);

            Console.WriteLine(dsContainer.ToString());
            Query queries = new(dsContainer);

            //CheckQueryResults queries = new CheckQueryResults(dataStorePath);


            using (HtmlGenerator generator = new(htmlOutput))
            {
                //generator.Write(dsContainer.DataStore);

                //LeagueDescription ld = queries.LeagueDescriptions.First();
                //generator.Write(ld);
                //var ldResults = CheckQueryResults<LeagueDescription>.CheckResults(ld);
                //generator.Write(ldResults);
                //Console.WriteLine(ldResults.ToString());

                //Player player = queries.Players.First();
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

                //Game game = queries.PlayedGames.First();
                //generator.Write(game);
                //var gameResults = CheckQueryResults<Game>.CheckResults(game);
                //generator.Write(gameResults);
                //Console.WriteLine(gameResults.ToString());


                var reportLeaguePlayers = ReportLeaguePlayers(queries, "Community", "Friday", generator);
                IEnumerable<Player> players = queries.GetLeaguePlayersSummary("Community", "Friday");
                generator.Write(players);

                //generator.DumpHtml("TestResults.html");
                generator.DumpHtml("TestResults.html",
                                   new string[] { "A Summary of all Friday Community players", "Testing \"Summary of all Friday Community players\"" },
                                   new string[] { "Stats for players", "Test Results" });

            }

            dsContainer.Dispose();
        }

        public static CheckQueryResults<IEnumerable<Player>> ReportLeaguePlayers(Query queries, string leagueCategory, string day, HtmlGenerator generator)
        {
            IEnumerable<Player> players = queries.LeaguePlayers(leagueCategory, day);
            generator.Write(players);
            var leaguePlayers = CheckQueryResults<IEnumerable<Player>>.CheckResults(players);
            generator.Write(leaguePlayers);
            return leaguePlayers;
        }
    }
}
