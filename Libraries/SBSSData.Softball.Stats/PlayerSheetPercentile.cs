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
            ZScore = 0;
            NumPlayers = 0;
        }

        public PlayerSheetPercentile(int numPlayers, 
                                     string playerName, 
                                     string propertyName = "", 
                                     double propertyValue = 0.0, 
                                     int rank = 0, 
                                     int percentile = 0,
                                     double zScore = 0.0)
        {
            PlayerName = playerName;
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            Rank = rank;
            Percentile = percentile;
            ZScore = zScore;
            NumPlayers = numPlayers;

        }

        public string PlayerName
        {
            get; set;
        }
        public string PropertyName
        {
            get; set;
        }
        public double PropertyValue
        {
            get; set;
        }
        public int Rank
        {
            get; set;
        }
        public int Percentile
        {
            get; set;
        }

        public double ZScore
        {
            get; set;
        }
        public int NumPlayers
        {
            get; set;
        }


        /// <summary>
        /// Not going to report 0th percentile. Embarrassing, so instead report 1.
        /// </summary>
        /// <returns></returns>
        public string PercentileToString()
        {
            int reportingPercentile = Percentile > 0 ? Percentile : 1;
            int lastDigit = reportingPercentile % 10;
            string suffix = "th";
            if ((reportingPercentile < 10) || (reportingPercentile > 20))
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

            return $"{reportingPercentile}{suffix}";
        }
    }
}
