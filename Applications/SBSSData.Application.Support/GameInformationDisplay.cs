using SBSSData.Softball;

namespace SBSSData.Application.Support
{
    public record class GameInformationDisplay(string Title,
                                               string GameId,
                                               //string? DataSource,
                                               string GameDate,
                                               string League)
    {
        public GameInformationDisplay(GameInformation gameInformation) : this
                                        (gameInformation.Title,
                                        gameInformation.GameId,
                                        //gameInformation.DataSource?.ToString(),
                                        gameInformation.Date.ToString("MMMM dd, yyyy a\\t h:mm tt"),
                                        $"{gameInformation.LeagueDay} {gameInformation.LeagueCategory} {gameInformation.Season}"
                                        )
        {
        }
    }
}
