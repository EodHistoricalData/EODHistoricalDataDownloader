using System;
using System.IO;
using System.Xml.Serialization;

namespace EODHistoricalDataDownloader.Program
{
    internal static class Settings
    {
        private static readonly string xmlFilename = "settings.xml";
        private static readonly string path;
        internal static SettingsFields? SettingsFields;

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
            try
            {
                if (!Directory.Exists(Program.UserFolder)) Directory.CreateDirectory(Program.UserFolder);
                XmlSerializer formatter = new(typeof(SettingsFields));
                using FileStream fs = new(path, FileMode.Create);
                formatter.Serialize(fs, SettingsFields);
            }
            catch
            {
                throw;
            }
        }
    }
}
