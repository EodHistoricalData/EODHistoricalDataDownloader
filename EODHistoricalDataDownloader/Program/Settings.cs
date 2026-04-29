using EODHistoricalDataDownloader.Model;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
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
            MigrateLegacyToGroups();
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
            SyncGroupsToLegacy();
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
                _debounceTimer = new Timer(_ =>
                {
                    if (Application.Current?.Dispatcher != null)
                        Application.Current.Dispatcher.Invoke(Save);
                    else
                        Save();
                }, null, 500, Timeout.Infinite);
            }
        }

        /// <summary>
        /// Migrate legacy flat EndOfDay fields into a single "Default" group
        /// if no groups exist yet. Called once after Read().
        /// </summary>
        private static void MigrateLegacyToGroups()
        {
            if (SettingsFields == null) return;

            if (SettingsFields.EndOfDayGroups != null && SettingsFields.EndOfDayGroups.Count > 0)
                return;

            var group = new DownloadGroup
            {
                Name = "Default",
                Period = SettingsFields.EndOfDayPeriod ?? "Daily",
                Format = SettingsFields.EndOfDayFormat ?? "Metastock",
                Output = SettingsFields.EndOfDayOutput ?? "Separate files",
                DateFrom = SettingsFields.EndOfDayFrom,
                DateTo = SettingsFields.EndOfDayTo,
                FilePath = SettingsFields.EndOfDayFilePath ?? "",
                IsUpdate = SettingsFields.EndOfDayIsUpdate,
                Tickers = SettingsFields.EndOfDayTickers ?? new List<string>()
            };

            SettingsFields.EndOfDayGroups = new List<DownloadGroup> { group };
        }

        /// <summary>
        /// Mirror the first group back to legacy flat fields for backward compat.
        /// Called before Save().
        /// </summary>
        private static void SyncGroupsToLegacy()
        {
            if (SettingsFields?.EndOfDayGroups == null || SettingsFields.EndOfDayGroups.Count == 0)
                return;

            var first = SettingsFields.EndOfDayGroups.First();
            SettingsFields.EndOfDayTickers = first.Tickers;
            SettingsFields.EndOfDayPeriod = first.Period;
            SettingsFields.EndOfDayFormat = first.Format;
            SettingsFields.EndOfDayOutput = first.Output;
            SettingsFields.EndOfDayFrom = first.DateFrom;
            SettingsFields.EndOfDayTo = first.DateTo;
            SettingsFields.EndOfDayFilePath = first.FilePath;
            SettingsFields.EndOfDayIsUpdate = first.IsUpdate;
        }
    }
}
