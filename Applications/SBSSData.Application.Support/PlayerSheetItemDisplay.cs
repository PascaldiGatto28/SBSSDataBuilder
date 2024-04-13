using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record PlayerSheetItemDisplay(string Description, List<PlayerDataDisplay> Totals)
    {
        public PlayerSheetItemDisplay(PlayerSheetItem playerSheetItem) : this
            (
                playerSheetItem.Description,
                new List<PlayerDataDisplay>()
                {
                    new PlayerDataDisplay(new PlayerData(playerSheetItem.PlayerTotals)),
                    new PlayerDataDisplay(new PlayerData(playerSheetItem.LeagueTotals))
                }
                //[new PlayerDataDisplay(new PlayerData(playerSheetItem.PlayerTotals)), new PlayerDataDisplay(new PlayerData(playerSheetItem.LeagueTotals))].ToList()
            )
        {
        }
    }
}
