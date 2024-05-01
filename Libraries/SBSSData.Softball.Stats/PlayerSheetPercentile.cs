namespace SBSSData.Softball.Stats
{
    public class PlayerSheetPercentile
    {
        public PlayerSheetPercentile()
        {
            PlayerName = "Unknown";
            PropertyName = "Unknown";
            PropertyValue = 0;
            Rank = 0;
            Percentile = 0;
            NumPlayers = 0;
        }

        public PlayerSheetPercentile(int numPlayers, string playerName, string propertyName="", double propertyValue=0.0, int rank=0, int percentile=0) 
        {
            PlayerName = playerName;
            PropertyName = propertyName;
            PropertyValue = propertyValue;  
            Rank = rank;
            Percentile = percentile;
            NumPlayers = numPlayers;

        }

        public string PlayerName { get; set; }
        public string PropertyName { get; set; }
        public double PropertyValue { get; set; }
        public int Rank { get; set; }
        public int Percentile { get; set; }
        public int NumPlayers {get; set; }

        public string PercentileToString()
        {
            int lastDigit = Percentile % 10;
            string suffix = "th";
            if ((Percentile < 10) || (Percentile > 20))
            {
                if (lastDigit == 1)
                {
                    suffix = "st";
                }
                else if (lastDigit == 2)
                {
                    suffix = "nd";
                }
                else if (lastDigit == 3)
                {
                    suffix = "rd";
                }
            }

            return $"{Percentile}{suffix}";

        }

    }
}
