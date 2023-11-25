﻿using Levaro.SBSoftball.Common;

namespace Levaro.Application.SBSSDataStore
{
    public sealed class AppSettings
    {
        private const string settingsPath = @"Configuration\Settings.json";

        private AppSettings()
        {
        }

        public static AppSettings Instance(string path = null)
        {
            string settingsLocation = path ?? AppSettings.settingsPath;

            return settingsLocation.Deserialize<AppSettings>();
        }

        public static AppSettings Settings => Instance(settingsPath);


        public string DataStoreFolder
        {
            get;
            init;
        }

        public string DataStoreFileName
        {
            get;
            init;
        }

        public string LogFileName
        {
            get;
            init;
        }

        public string DataStorePath => $"{DataStoreFolder}{DataStoreFileName}";

        public string LogFilePath => $"{DataStoreFolder}{LogFileName}";
    }
}