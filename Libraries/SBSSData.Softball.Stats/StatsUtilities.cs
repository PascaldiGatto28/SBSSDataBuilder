namespace SBSSData.Softball.Stats
{
    public static class StatsUtilities
    {
        public static Player SetName(this Player player, string? name = null)
        {
            if ((player != null) && !string.IsNullOrEmpty(name))
            {
                typeof(Player).GetProperty("Name")?.SetValue(player, name, null);
            }

            return player;
        }
    }
}
