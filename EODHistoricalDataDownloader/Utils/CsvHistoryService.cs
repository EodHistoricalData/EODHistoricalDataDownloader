using System;
using System.Globalization;
using System.IO;

namespace EODLoader.Services.Utils
{
    public class CsvHistoryService : ICsvHistoryService
    {
        private static readonly string[] DateFormats =
        {
            "yyyy-MM-dd", "yyyyMMdd", "M/d/yyyy",
            "M/d/yyyy h:mm:ss tt", "M/d/yyyy H:mm:ss",
            "MM/dd/yyyy", "dd/MM/yyyy", "dd.MM.yyyy"
        };

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

                foreach (var rawField in lastLine.Split(','))
                {
                    var field = rawField.Trim().Trim('"');
                    if (field.Length < 6) continue;

                    if (DateTime.TryParseExact(field, DateFormats,
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                    {
                        return date;
                    }
                }

                foreach (var rawField in lastLine.Split(','))
                {
                    var field = rawField.Trim().Trim('"');
                    if (field.Length < 6) continue;

                    if (DateTime.TryParse(field, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var fallbackDate))
                    {
                        return fallbackDate;
                    }
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
