using System;
using System.Globalization;
using System.IO;

namespace EODLoader.Services.Utils
{
    public class CsvHistoryService : ICsvHistoryService
    {
        private static readonly string[] DateFormats = { "yyyy-MM-dd", "yyyyMMdd", "M/d/yyyy" };

        public DateTime? GetLastDate(string csvPath)
        {
            try
            {
                if (!File.Exists(csvPath))
                    return null;

                var info = new FileInfo(csvPath);
                if (info.Length < 10)
                    return null;

                string? lastLine = null;
                foreach (var line in File.ReadLines(csvPath))
                {
                    if (!string.IsNullOrWhiteSpace(line))
                        lastLine = line;
                }

                if (string.IsNullOrWhiteSpace(lastLine))
                    return null;

                var firstField = lastLine.Split(',')[0].Trim().Trim('"');

                if (DateTime.TryParseExact(firstField, DateFormats,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }

                if (DateTime.TryParse(firstField, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var fallbackDate))
                {
                    return fallbackDate;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
