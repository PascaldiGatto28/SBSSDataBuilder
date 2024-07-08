using System.Data;
using System.Diagnostics;

using SBSSData.Softball;
using SBSSData.Softball.Common;
using SBSSData.Softball.Logging;

namespace TestCreateFullDataStore
{
    public class Program
    {
        internal static void Main()
        {
            string logPath = @"D:\Temp\Junk\MoreJunk\x.log";
            Log log = new(logPath);
            //string path = @"D:\Temp\junk\LeaguesData.json";
            //Program.Build(path);
            // _ = Program.Update(path, log);
            //Log log = new(logPath);

            LogEntry logEntry = new()
            {
                SessionId = Guid.NewGuid(),
                Date = DateTime.Parse("June 6, 1945"),
                LogCategory = LogCategory.Warning,
                LogText = "Test is a test using \"void WriteLine(LogEntry entry)\"",
                ObjectInstance = new string('?', 32),
                CallerFileName = "FooBar.txt",
                CallerMemberName = "Foo",
                CallerLineNumber = int.MaxValue
            };

            DateTime now = DateTime.Now;
            log.WriteLine(logEntry);
            log.WriteLine(now.AddDays(1), LogCategory.Debug, "\"void WriteLine(DateTime dateTime, LogCategory category, string text, object? instance\"", null);
            log.WriteLine("\"void WriteLine(string text, object instance)\"", new object());
            log.WriteLine(LogCategory.Info, "\"void WriteLine(LogCategory category, string text, object instance\"", new Exception("shit"));
            log.WriteLine(LogCategory.Warning, "Warning, \"void WriteLine(LogCategory category, string text)\"");
            log.WriteLine("\"void WriteLine(string text);\"");
            log.WriteLine("\"void WriteLine(string text, Exception exception)\"", new InvalidOperationException("Yes, it's not so"));
            log.WriteLine(new Exception("\"void WriteLine(Exception exception\""));
            log.Stop();
            log.Close();

            IEnumerable<LogSession> sessions = Log.ReadLog(logPath);
            Console.WriteLine(sessions.ToJsonString());
            //sessions.Serialize(logPath);
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


        public static LeaguesData Update(string path, Log log)
        {
            LeaguesData dataStore = path.Deserialize<LeaguesData>();
            int count = dataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).Count(g => g.IsComplete);
            log.WriteLine($"There are {count} complete games as of {dataStore.BuildDate:dddd MMMM d, yyyy a\\t h:mm tt}");
            IEnumerable<LeagueSchedule> schedules = dataStore.LeagueSchedules;
            int updated = 0;
            IEnumerable<ScheduledGame> scheduledGames = schedules.SelectMany((s) => s.ScheduledGames);

            foreach (ScheduledGame scheduledGame in scheduledGames.Where(s => !s.IsComplete))
            {
                //DateTime recordedTime = scheduledGame.Date.AddHours(checkHours - scheduledGame.Date.Hour);
                if (scheduledGame.IsRecorded)
                {
                    log.WriteLine($"Processing {scheduledGame.GameResults.GameInformation}");
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
                        // When the game date is less than the build date, and not complete the game has been canceled
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
                log.WriteLine($"{updated} games updated, data store serialized using {size:#,###} bytes.");
                dataStore = path.Deserialize<LeaguesData>();
                count = dataStore.LeagueSchedules.SelectMany(s => s.ScheduledGames).Count(g => g.IsComplete);
                log.WriteLine($"There are {count} complete games as of {dataStore.BuildDate:dddd MMMM d, yyyy a\\t h:mm tt}");
            }
            else
            {
                log.WriteLine("No updates made");
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