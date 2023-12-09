using Levaro.SBSoftball.Logging;

namespace Levaro.Application.SBSSDataStore
{
    /// <summary>
    /// Contains objects uses to both configure the application and provide access to processing data.
    /// </summary>
    public sealed class AppContext
    {
        private static Lazy<AppContext> lazy = new(() => new AppContext());

        public static AppContext Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private AppContext()
        {
            Settings = AppSettings.Settings;
            //LogFilePath = Settings.LogFilePath;
            Log = new Log(Settings.LogFilePath);
        }

        public static AppContext Reset()
        {
            lazy = new Lazy<AppContext>(() => new AppContext());
            return Instance;
        }

        public AppSettings Settings
        {
            get;
            init;
        }


        public Log Log
        {
            get;
            init;
        }

    }
}
