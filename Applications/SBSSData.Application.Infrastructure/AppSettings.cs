using System.Reflection;

using SBSSData.Softball.Common;

namespace SBSSData.Application.Infrastructure
{
    /// <summary>
    /// Recovers application settings from a JSON file (<c>Settings.json</c>) used to configure the application and data store
    /// processing. A created instance of this class is a singleton.
    /// </summary>
    public sealed class AppSettings
    {
        /// <summary>
        /// The default location of the settings JSON file that is deserialized to create an instance of <c>AppSettings</c> 
        /// relative to folder of the executable application.
        /// </summary>
        private const string settingsPath = @"Configuration\Settings.json";

        /// <summary>
        /// The singleton instance of <see cref="AppSettings"/>; it is only created when its value is null.
        /// </summary>
        private static AppSettings? instance;

        /// <summary>
        /// The default constructor and because it is private, instances can only be created using the static 
        /// <see cref="Instance(string)"/> method.
        /// </summary>
        private AppSettings()
        {
            DataStoreFolder = string.Empty;
            DataStoreFileName = string.Empty;
            LogFileName = string.Empty;
            LogSessionFileName = string.Empty;
            BuildOption = string.Empty;
            HtmlFolder = string.Empty;
            Test = false;
            Season = string.Empty;
        }

        /// <summary>
        /// Returns the instance of this class. If no instance yet exists, one is created and all calls to this method return
        /// the same instance.
        /// </summary>
        /// <remarks>
        /// You can force another instance to be created, by first calling the static <see cref="Reset()"/>.
        /// </remarks>
        /// <param name="path">This optional parameter is full path to a JSON file that can be deserialized to an instance
        /// of this <c>AppSettings</c>. If the parameter is not used, the default location is used which is a path relative
        /// the folder where the executable resides. The default value is <c>Configuration\Settings.json</c></param>
        /// <returns>The existing <c>AppSettings</c> instance or a new one if it does not exist.
        /// </returns>
        /// <exception cref="InvalidOperationException">if there's an error when trying to deserialized a specified path.
        /// This can occur if the file path is not well-formed or the file cannot be serialized. The error that causes
        /// the problem can be found in the inner exception.</exception>
        public static AppSettings Instance(string? path = null)
        {
            // N.B. We need to get the full path because when running as a scheduled task, the directory is relative
            // to the scheduler executable.
            string defaultPath = @$"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}\{settingsPath}";
            string settingsLocation = string.IsNullOrEmpty(path) ? defaultPath : path;
            if (instance == null)
            {
                try
                {
                    instance = settingsLocation.Deserialize<AppSettings>() ?? defaultPath.Deserialize<AppSettings>();
                    if (instance == null)
                    {
                        string message = $"{defaultPath} is the default path";
                        throw new ArgumentNullException(message);
                    }
                }
                catch (Exception exception)
                {
                    string message = "Could not create an AppSettings instance, see the \"innerException\" for more details.";
                    throw new InvalidOperationException(message, exception);
                }
            }

            return instance;
        }


        /// <summary>
        /// Returns the current (or new) <c>AppSettings</c> instance.
        /// </summary>
        /// <remarks>This is just a convenience method to create and return the settings using the default configuration file.
        /// For example, <see cref="AppContext"/> uses this property to create its own settings property:
        /// <code language="cs">
        /// AppSettings settings = AppContext.Settings;
        /// // So consumers of the AppSettings class, can gain access through AppContext, like so:
        /// settings = AppContext.Settings;
        /// string dataStorePath = settings.DataStorePath;
        /// </code>
        /// </remarks>
        public static AppSettings Settings => Instance();

        /// <summary>
        /// Sets the private instance field to <c>null</c>. The next call to <see cref="AppSettings.Instance(string?)"/> will
        /// attempt to create and return a new instance.
        /// </summary>
        public static void Reset() => instance = null;

        /// <summary>
        /// Gets and initializes the data store folder location when the <see cref="AppSettings"/> is deserialized. 
        /// </summary>
        public string DataStoreFolder
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the season name. It is of the YYYY CalendarSeason, for example "Spring 2024"
        /// </summary>
        public string Season
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the data store file name when the <see cref="AppSettings"/> is deserialized.
        /// </summary>
        public string DataStoreFileName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the log file name when the <see cref="AppSettings"/> is deserialized. The log file records all
        /// the messages for the duration of the log's lifetime. 
        /// </summary>
        /// <seealso cref="LogFilePath"/>
        public string LogFileName
        {
            get;
            init;
        }

        /// <summary>
        /// Gets and initializes the log file name when the <see cref="AppSettings"/> is deserialized. The log file records all
        /// the messages for the duration of the log's lifetime. 
        /// </summary>
        /// <seealso cref="LogSessionFilePath"/>
        public string LogSessionFileName
        {
            get;
            init;
        }

        /// <summary>
        /// Specifies how the data store should be constructed: build a new data store, or update the existing data store.
        /// </summary>
        public string BuildOption
        {
            get;
            init;
        }

        /// <summary>
        /// Specifies if the Web publishes to a test site of the production folder on the web server
        /// </summary>
        public bool Test
        {
            get;
            init;
        }

        /// <summary>
        /// Specifies where the constructed HTML files are relative to the Data Store folder.
        /// </summary>
        public string HtmlFolder
        {
            get;
            init;
        }

        /// <summary>
        /// Gets the data store path; it is the concatenation of the <see cref="DataStoreFolder"/> and 
        /// the <see cref="DataStoreFileName"/> properties.
        /// </summary>
        /// <remarks>
        /// When a data store is created up date, the <c>DataStoreFolder</c> is the location of the data store, the log file and
        /// generated log session file
        /// </remarks>
        public string DataStorePath
        {
            get
            {
                return $"{DataStoreFolder}{Season.RemoveWhiteSpace()}{DataStoreFileName}";
            }
        }

        /// <summary>
        /// Gets the log file name; it is the concatenation of the <see cref="DataStoreFolder"/> and 
        /// the <see cref="LogFileName"/> properties.
        /// </summary>
        /// <remarks>
        /// Notice that the data store and log file output are same folder.
        /// </remarks>
        public string LogFilePath => $"{DataStoreFolder}{LogFileName}";

        /// /// <summary>
        /// Gets the log session path; it is the concatenation of the <see cref="DataStoreFolder"/> and 
        /// the <see cref="LogSessionFileName"/> properties.
        /// </summary>
        /// <remarks>
        /// The log session file is constructed by reading the log file and organizing it by session and by descending data
        /// (most recent are first). It is a JSON when deserialized is a sequence of <see cref="SBSSData.Softball.Logging.LogSession"/>
        /// objects.
        /// </remarks>
        public string LogSessionFilePath => $"{DataStoreFolder}{LogSessionFileName}";

        /// <summary>
        /// Returns a value indicating the build option is to update.
        /// </summary>
        public bool Update => !string.Equals(BuildOption, "Build", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Returns the full path of the folder containing the constructed HTML files
        /// </summary>
        public string HtmlLocation => $"{DataStoreFolder}{HtmlFolder}";

        /// <summary>
        /// Returns the <c>true</c> if deployment to the TestSync folder on the web server; <c>false</c> if deployment is to
        /// the production server folder. The returned value is just the <see cref="Test"/> property.
        /// </summary>
        public bool IsTest => Test;

    }
}
