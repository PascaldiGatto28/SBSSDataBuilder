namespace Levaro.SBSoftball.Logging
{
    public enum LogCategory
    {
        Unknown = 0,
        StartSession = Unknown + 1,
        EndSession = StartSession + 1,
        Info = EndSession + 1,
        Debug = Info + 1,
        Warning = Debug + 1,
        Error = Warning + 1
    }
}
