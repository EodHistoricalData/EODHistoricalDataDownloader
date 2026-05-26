using System;
using System.Collections.Generic;

namespace EODHistoricalDataDownloader.Model
{
    [Serializable]
    public class DownloadGroup
    {
        public string Id = Guid.NewGuid().ToString();
        public string Name = "Default";
        public List<string> Tickers = new();
        public string FilePath = "";
        public string Period = "Daily";
        public string Format = "Default";
        public string Output = "Separate files";
        public bool Adjusted;
        public DateTime DateFrom = DateTime.Today;
        public DateTime DateTo = DateTime.Today;
        public bool IsUpdate;
    }
}
