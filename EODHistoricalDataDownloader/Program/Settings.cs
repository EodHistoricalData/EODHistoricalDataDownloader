using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace EODHistoricalDataDownloader.Program
{
    internal static class Settings
    {
        private static readonly string xmlFilename = "settings.xml";
        private static readonly string path;
        internal static SettingsFields? SettingsFields;

        private static Timer? _debounceTimer;
        private static readonly object _saveLock = new();

        static Settings()
        {
            SettingsFields = new SettingsFields();
            path = Path.Combine(Program.UserFolder, xmlFilename);
            Read();
        }

        /// <summary>
        /// Чтение настроек
        /// </summary>
        internal static void Read()
        {
            if (!File.Exists(path)) Save();

            try
            {
                XmlSerializer formatter = new(typeof(SettingsFields));
                using FileStream fs = File.OpenRead(path);
                SettingsFields = (SettingsFields)formatter.Deserialize(fs);
            }
            catch (Exception)
            {
                Save();
            }
        }

        /// <summary>
        /// Сохранение настроек
        /// </summary>
        internal static void Save()
        {
            if (!Directory.Exists(Program.UserFolder)) Directory.CreateDirectory(Program.UserFolder);
            XmlSerializer formatter = new(typeof(SettingsFields));
            using FileStream fs = new(path, FileMode.Create);
            formatter.Serialize(fs, SettingsFields);
        }

        internal static void SaveDebounced()
        {
            lock (_saveLock)
            {
                _debounceTimer?.Dispose();
                _debounceTimer = new Timer(_ => Save(), null, 500, Timeout.Infinite);
            }
        }
    }
}
