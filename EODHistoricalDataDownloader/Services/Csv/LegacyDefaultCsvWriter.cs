using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EODHistoricalDataDownloader.Services.Csv
{
    /// <summary>
    /// Faithful reproduction of pre-2.1.1 output: header row + comma-separated values,
    /// en-US culture, datetime in the Date column (with "T00:00:00" suffix on EOD bars
    /// because the wrapper exposes DateTime). Adjusted_close kept as its own column.
    /// In OneFile mode the Ticker column is appended (matching old reflection order).
    /// </summary>
    public sealed class LegacyDefaultCsvWriter : CsvWriterBase, ICsvWriter
    {
        public Task WriteEndOfDayAsync(string path, IEnumerable<EndOfDayRow> rows, bool append, bool adjusted, bool oneFile)
        {
            return Task.Run(() =>
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = OutputCulture;
                bool emitTicker = oneFile;
                using var writer = new StreamWriter(File.Open(path, append ? FileMode.Append : FileMode.Create));

                if (!append)
                {
                    writer.WriteLine(emitTicker
                        ? "Date,Open,High,Low,Close,Adjusted_close,Volume,Ticker"
                        : "Date,Open,High,Low,Close,Adjusted_close,Volume");
                }

                foreach (var row in ApplyAdjustment(rows, adjusted))
                {
                    var parts = new List<string>
                    {
                        row.Date.ToString(OutputCulture),
                        FormatDecimal(row.Open),
                        FormatDecimal(row.High),
                        FormatDecimal(row.Low),
                        FormatDecimal(row.Close),
                        FormatDecimal(row.AdjustedClose),
                        FormatLong(row.Volume)
                    };
                    if (emitTicker) parts.Add(row.Ticker);
                    writer.WriteLine(string.Join(",", parts));
                }
            });
        }

        public Task WriteIntradayAsync(string path, IEnumerable<IntradayRow> rows, bool append, bool oneFile)
        {
            return Task.Run(() =>
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = OutputCulture;
                bool emitTicker = oneFile;
                using var writer = new StreamWriter(File.Open(path, append ? FileMode.Append : FileMode.Create));

                if (!append)
                {
                    writer.WriteLine(emitTicker
                        ? "Timestamp,Gmtoffset,DateTime,Open,High,Low,Close,Volume,Ticker"
                        : "Timestamp,Gmtoffset,DateTime,Open,High,Low,Close,Volume");
                }

                foreach (var row in rows)
                {
                    var parts = new List<string>
                    {
                        FormatLong(row.Timestamp),
                        row.GmtOffset.HasValue ? row.GmtOffset.Value.ToString(OutputCulture) : string.Empty,
                        row.DateTime.ToString(OutputCulture),
                        FormatDecimal(row.Open),
                        FormatDecimal(row.High),
                        FormatDecimal(row.Low),
                        FormatDecimal(row.Close),
                        FormatLong(row.Volume)
                    };
                    if (emitTicker) parts.Add(row.Ticker);
                    writer.WriteLine(string.Join(",", parts));
                }
            });
        }
    }
}
