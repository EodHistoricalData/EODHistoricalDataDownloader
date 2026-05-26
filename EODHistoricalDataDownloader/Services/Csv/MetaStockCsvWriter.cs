using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EODHistoricalDataDownloader.Services.Csv
{
    /// <summary>
    /// MetaStock ASCII import format per customer ticket:
    ///   &lt;Name&gt;_&lt;Symbol&gt;,YYYYMMDD,Open,High,Low,Close,Volume
    /// No header, no Adjusted_close column, no time component, en-US decimals.
    /// </summary>
    public sealed class MetaStockCsvWriter : CsvWriterBase, ICsvWriter
    {
        public Task WriteEndOfDayAsync(string path, IEnumerable<EndOfDayRow> rows, bool append, bool adjusted, bool oneFile)
        {
            return Task.Run(() =>
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = OutputCulture;
                using var writer = new StreamWriter(File.Open(path, append ? FileMode.Append : FileMode.Create));

                foreach (var row in ApplyAdjustment(rows, adjusted))
                {
                    string nameSymbol = BuildNameSymbol(row.Ticker, row.Name);

                    writer.WriteLine(string.Join(",",
                        nameSymbol,
                        row.Date.ToString("yyyyMMdd", OutputCulture),
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
                    string nameSymbol = BuildNameSymbol(row.Ticker, row.Name);

                    writer.WriteLine(string.Join(",",
                        nameSymbol,
                        row.DateTime.ToString("yyyyMMdd", OutputCulture),
                        row.DateTime.ToString("HHmmss", OutputCulture),
                        FormatDecimal(row.Open),
                        FormatDecimal(row.High),
                        FormatDecimal(row.Low),
                        FormatDecimal(row.Close),
                        FormatLong(row.Volume)));
                }
            });
        }

        /// <summary>
        /// Compose the "&lt;Name&gt;_&lt;Symbol&gt;" first field. Strips characters that
        /// would corrupt the CSV row (commas, quotes, embedded underscores in the name,
        /// and any control whitespace) so the field is always a single safe token.
        /// </summary>
        private static string BuildNameSymbol(string ticker, string? name)
        {
            string symbol = SymbolUtils.StripExchange(ticker);
            string raw = string.IsNullOrWhiteSpace(name) ? symbol : name!;
            string clean = raw
                .Replace(",", "")
                .Replace("\"", "")
                .Replace("\r", "")
                .Replace("\n", "")
                .Trim();
            if (string.IsNullOrEmpty(clean)) clean = symbol;
            return $"{clean}_{symbol}";
        }
    }
}
