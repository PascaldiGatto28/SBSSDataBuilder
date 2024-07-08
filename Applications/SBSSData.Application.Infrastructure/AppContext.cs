using SBSSData.Softball.Logging;

namespace SBSSData.Application.Infrastructure
{
    /// <summary>
    /// Contains objects used to both configure (<see cref="AppSettings"/>) the application and provide access to 
    /// logging (<see cref="Log"/>).
    /// </summary>
    public sealed class AppContext
    {
        /// <summary>
        /// Allows the construction of this singleton class to be deferred until needed.
        /// </summary>
        private static Lazy<AppContext> lazy = new(() => new AppContext());

        /// <summary>
        /// The single instance of <see cref="AppContext"/>; it is only created when its value is null.
        /// </summary>
        private static AppContext? instance;

        /// <summary>
        /// Returns a singleton instance of the <see cref="AppContext"/> class.
        /// </summary>
        public static AppContext Instance
        {
            get
            {
                return instance ??= lazy.Value;
            }
        }

        /// <summary>
        /// This private constructor is called by the <see cref="Instance"/> property if necessary.
        /// </summary>
        /// <remarks>
        /// <see cref="AppSettings"/> and <see cref="Log"/> properties are initialized.
        /// </remarks>
        private AppContext()
        {
            Settings = AppSettings.Settings;
            Log = new Log(Settings.LogFilePath);
        }

        /// <summary>
        /// Resets the <see cref="Instance"/> by invoking the private constructor. Normally this is not
        /// needed.
        /// </summary>
        /// <returns></returns>
        public static AppContext Reset()
        {
            lazy = new Lazy<AppContext>(() => new AppContext());
            return Instance;
        }

        /// <summary>
        /// Gets and initializes the <c>Settings</c> property; it is constructed when an
        /// <see cref="Instance"/> is created.
        /// </summary>
        /// <seealso cref="AppSettings"/>
        public AppSettings Settings
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the <c>Log</c> property; it is constructed when an
        /// <see cref="Instance"/> is created.
        /// </summary>
        /// <seealso cref="Log"/>
        public Log Log
        {
            get;
            init;
        }
    }
}
