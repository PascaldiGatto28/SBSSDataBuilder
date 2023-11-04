namespace Levaro.SBSoftball
{
    public class Player
    {
        public Player()
        {
            Name = string.Empty;
        }

        public string Name
        {
            get;
            set;
        }

        public int AtBats
        {
            get;
            set;
        }

        public int Runs
        {
            get;
            set;
        }

        public int Singles
        {
            get;
            set;
        }

        public int Doubles
        {
            get;
            set;
        }

        public int Triples
        {
            get;
            set;
        }

        public int HomeRuns
        {
            get;
            set;
        }
        public int BasesOnBalls
        {
            get; set;
        }

        public int SacrificeFlies
        {
            get;
            set;
        }
    }
}