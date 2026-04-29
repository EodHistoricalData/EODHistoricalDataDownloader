using EODHistoricalDataDownloader.Model;

using System;
using System.Collections.Generic;

namespace EODHistoricalDataDownloader.Program
{
    [Serializable]
    public class SettingsFields
    {
        /// <summary>
        /// API ключ
        /// </summary>
        public string APIKey = "demo";
        public int MaxThreads = 3;
        public bool UseProxy;
        public string ProxyHost = "";
        public bool WithCredentials;
        public string ProxyUsername = "";
        public string ProxyPassword = "";

        // Legacy flat fields — kept for backward compatibility with old settings.xml
        public List<string>? EndOfDayTickers;
        public string EndOfDayPeriod = "Daily";
        public string EndOfDayFormat = "Metastock";
        public string EndOfDayOutput = "Separate files";
        public DateTime EndOfDayFrom = DateTime.Today;
        public DateTime EndOfDayTo = DateTime.Today;
        public string EndOfDayFilePath = "";
        public bool EndOfDayIsUpdate;

        // Download groups (new)
        public List<DownloadGroup>? EndOfDayGroups;

        public List<string>? IntradayTickers;
        public string IntradayInterval = "5 minutes";
        public string IntradayFormat = "Metastock";
        public string IntradayOutput = "Separate files";
        public DateTime IntradayFrom = DateTime.Today;
        public DateTime IntradayTo = DateTime.Today;
        public string IntradayFilePath = "";
        public bool IntradayIsUpdate;

        public SettingsFields()
        {

        }
    }
}
