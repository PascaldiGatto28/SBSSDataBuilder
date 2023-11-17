using System.Data;
using System.Diagnostics;

using Levaro.SBSoftball;
using Levaro.SBSoftball.Common;

namespace TestCreateFullDataStore
{
    public class Program
    {
        internal static void Main()
        {
            //Program program = new();
            string path = @"D:\Temp\Junk\LeaguesData.json";
            //Program.Build(path);
            _ = Program.Update(path);
        }

        public static LeaguesData Build(string path)
        {
            static void callback(string m) => Support.ProcessMessage(m);
            LeaguesData leaguesData = LeaguesData.ConstructLeaguesData(message: callback);
            //leaguesData.Dump("Constructed");
            //string json = leaguesData.ToString();
            //string path = @"D:\Temp\Junk\LeaguesData.json";
            int size = leaguesData.Serialize(path);
            Console.WriteLine($"LeaguesData serialized {size:#,###} bytes to {path}");

            return path.Deserialize<LeaguesData>();
        }


        public static LeaguesData Update(string path)
        {
            LeaguesData dataStore = path.Deserialize<LeaguesData>();
            int count = dataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).Count(g => g.IsComplete);
            Console.WriteLine($"There are {count} complete games as of {dataStore.BuildDate:dddd MMMM d, yyyy a\\t h:mm tt}");
            IEnumerable<LeagueSchedule> schedules = dataStore.LeagueSchedules;
            int updated = 0;
            IEnumerable<ScheduledGame> scheduledGames = schedules.SelectMany((s) => s.ScheduledGames);

            foreach (ScheduledGame scheduledGame in scheduledGames.Where(s => !s.IsComplete))
            {
                //DateTime recordedTime = scheduledGame.Date.AddHours(checkHours - scheduledGame.Date.Hour);
                if (scheduledGame.IsRecorded)
                {
                    Console.WriteLine($"Processing {scheduledGame.GameResults.GameInformation}");
                    Game currentGame = scheduledGame.GameResults;
                    Game updatedGame = Game.ConstructGame(scheduledGame, update: true);
                    scheduledGame.GameResults = updatedGame;
                    if (updatedGame.Teams.Count == 2)
                    {
                        scheduledGame.VisitorScore = updatedGame.Teams[0].RunsScored;
                        scheduledGame.HomeScore = updatedGame.Teams[1].RunsScored;
                    }
                    else
                    {
                        // For now on it will show completed so it won't be processed again.
                        // When the game date is less than the build date, and not complete the game has been cancelled
                        // and will never be played.
                        scheduledGame.VisitorScore = 0;
                        scheduledGame.HomeScore = 0;
                    }

                    dataStore.BuildDate = DateTime.Now;
                    updated++;
                }
            }
            //}

            if (updated > 0)
            {
                int size = dataStore.Serialize<LeaguesData>(path);
                Console.WriteLine($"{updated} games updated, data store serialized using {size:#,###} bytes.");
                dataStore = path.Deserialize<LeaguesData>();
                count = dataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).Count(g => g.IsComplete);
                Console.WriteLine($"There are {count} complete games as of {dataStore.BuildDate:dddd MMMM d, yyyy a\\t h:mm tt}");
            }
            else
            {
                Console.WriteLine("No updates made");
            }

            return dataStore;
        }
    }

    public static class Support
    {
        private static Stopwatch sw = null;

        public static void ProcessMessage(string message)
        {
            if (sw == null)
            {
                sw = new Stopwatch();
                sw.Start();
            }

            if (string.IsNullOrEmpty(message) || (message == "END"))
            {
                sw.Stop();
            }
            else
            {
                message += $" Elapsed time {sw.ElapsedMilliseconds:#,##0} milliseconds";
                Console.WriteLine(message);
            }
        }

    }


}