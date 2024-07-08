using SBSSData.Softball;
using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record PlayerCardDataSummary(Player PlayerSummary, Player PlayersSummary, string Header)
    {
        public List<PlayerDataDisplay> DisplaySummary()
        {
            PlayerDataDisplay playerDisplay = ToDisplay(PlayerSummary);
            PlayerDataDisplay summaryDisplay = ToDisplay(PlayersSummary);
            return [playerDisplay, summaryDisplay];
        }

        private static PlayerDataDisplay ToDisplay(Player player)
        {
            return new PlayerDataDisplay(new PlayerData(player));
        }
    }
}
