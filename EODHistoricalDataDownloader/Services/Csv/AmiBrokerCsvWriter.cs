using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EODHistoricalDataDownloader.Services.Csv
{
    /// <summary>
    /// AmiBroker ASCII import (safe-default shape, permissive importer):
    ///   &lt;Ticker&gt;,YYYY-MM-DD,Open,High,Low,Close,Volume
    /// No header, dash-separated date, en-US decimals.
    /// </summary>
    public sealed class AmiBrokerCsvWriter : CsvWriterBase, ICsvWriter
    {
        public Task WriteEndOfDayAsync(string path, IEnumerable<EndOfDayRow> rows, bool append, bool adjusted, bool oneFile)
        {
            return Task.Run(() =>
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = OutputCulture;
                using var writer = new StreamWriter(File.Open(path, append ? FileMode.Append : FileMode.Create));

                foreach (var row in ApplyAdjustment(rows, adjusted))
                {
                    string symbol = SymbolUtils.StripExchange(row.Ticker);

                    writer.WriteLine(string.Join(",",
                        symbol,
                        row.Date.ToString("yyyy-MM-dd", OutputCulture),
                        FormatDecimal(row.Open),
                        FormatDecimal(row.High),
                        FormatDecimal(row.Low),
                        FormatDecimal(row.Close),
                        FormatLong(row.Volume)));
                }
            });
        }

        public Task WriteIntradayAsync(string path, IEnumerable<IntradayRow> rows, bool append, bool oneFile)
        {
            return Task.Run(() =>
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = OutputCulture;
                using var writer = new StreamWriter(File.Open(path, append ? FileMode.Append : FileMode.Create));

                foreach (var row in rows)
                {
                    string symbol = SymbolUtils.StripExchange(row.Ticker);

                    writer.WriteLine(string.Join(",",
                        symbol,
                        row.DateTime.ToString("yyyy-MM-dd", OutputCulture),
                        row.DateTime.ToString("HH:mm:ss", OutputCulture),
                        FormatDecimal(row.Open),
                        FormatDecimal(row.High),
                        FormatDecimal(row.Low),
                        FormatDecimal(row.Close),
                        FormatLong(row.Volume)));
                }
            });
        }
    }
}
