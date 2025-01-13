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
        public string ProxyHost;
        public bool WithCredentials;
        public string ProxyUsername;
        public string ProxyPassword;

        public List<string>? EndOfDayTickers;
        public string EndOfDayPeriod;
        public string EndOfDayFormat;
        public string EndOfDayOutput;
        public DateTime EndOfDayFrom = DateTime.Today;
        public DateTime EndOfDayTo = DateTime.Today;
        public string EndOfDayFilePath;
        public bool EndOfDayIsUpdate;

        public List<string>? IntradayTickers;
        public string IntradayInterval;
        public string IntradayFormat;
        public string IntradayOutput;
        public DateTime IntradayFrom = DateTime.Today;
        public DateTime IntradayTo = DateTime.Today;
        public string IntradayFilePath;
        public bool IntradayIsUpdate;

        public SettingsFields()
        {

        }
    }
}
