using System;

namespace EODHistoricalDataDownloader.Services.Csv
{
    /// <summary>
    /// Format-neutral intraday bar. Writers translate this into their target shape.
    /// </summary>
    public sealed record IntradayRow(
        string Ticker,
        string? Name,
        DateTime DateTime,
        long? Timestamp,
        int? GmtOffset,
        decimal? Open,
        decimal? High,
        decimal? Low,
        decimal? Close,
        long? Volume);
}
