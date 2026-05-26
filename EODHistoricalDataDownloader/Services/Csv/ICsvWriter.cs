using System.Collections.Generic;
using System.Threading.Tasks;

namespace EODHistoricalDataDownloader.Services.Csv
{
    /// <summary>
    /// Output-format strategy: one implementation per supported CSV shape.
    /// `oneFile` is forwarded so writers that care (Default's optional Ticker column)
    /// can adjust; writers that always emit a symbol per row (MetaStock, AmiBroker) ignore it.
    /// </summary>
    public interface ICsvWriter
    {
        Task WriteEndOfDayAsync(string path, IEnumerable<EndOfDayRow> rows, bool append, bool adjusted, bool oneFile);
        Task WriteIntradayAsync(string path, IEnumerable<IntradayRow> rows, bool append, bool oneFile);
    }
}
