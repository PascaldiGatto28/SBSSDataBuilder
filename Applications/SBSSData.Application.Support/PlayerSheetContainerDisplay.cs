using SBSSData.Softball.Stats;

namespace SBSSData.Application.Support
{
    public record PlayerSheetContainerDisplay(string Introduction, List<PlayerSheetItemDisplay> SheetItems)
    {
        public PlayerSheetContainerDisplay(PlayerSheetContainer psc) : this(psc.Introduction,
                                                                            psc.PlayerSheetItems.Select(s => new PlayerSheetItemDisplay(s)).ToList())
        {
        }
    }
}
