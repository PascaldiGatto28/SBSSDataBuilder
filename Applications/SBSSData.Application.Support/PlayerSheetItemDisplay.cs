using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record PlayerSheetItemDisplay(string Description, List<PlayerStatsDisplay> Totals)
    {
        public PlayerSheetItemDisplay(PlayerSheetItem playerSheetItem) : this
            (
                playerSheetItem.Description,
                new List<PlayerStatsDisplay>()
                {
                    new PlayerStatsDisplay(new(playerSheetItem.PlayerTotalsStats)),
                    new PlayerStatsDisplay(new(playerSheetItem.LeagueTotalsStats))
                }
                //[new PlayerDataDisplay(new PlayerData(playerSheetItem.PlayerTotals)), new PlayerDataDisplay(new PlayerData(playerSheetItem.LeagueTotals))].ToList()
            )
        {
        }
    }
}
