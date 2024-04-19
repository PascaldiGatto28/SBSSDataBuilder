using SBSSData.Application.LinqPadQuerySupport;

namespace SBSSData.Application.WebDeployment
{
    internal class Build
    {
        private static void Main(string[] args)
        {
            string[] seasons = ["2024 Winter", "2023 Fall", "2023 Summer"];
            foreach (string season in seasons)
            {
                Construction construction = new(season);
                construction.Build<LogSessions>(true);
                construction.Build<GamesTeamPlayersV3>(true);
                construction.Build<GamesTeamPlayersHelpV3>(true);
                construction.Build<PlayerSheets>(true);
                construction.Build<PlayerSheetsGuide>(true);
            }   



            //construction.WinSCPSync();
        }
    }
}
