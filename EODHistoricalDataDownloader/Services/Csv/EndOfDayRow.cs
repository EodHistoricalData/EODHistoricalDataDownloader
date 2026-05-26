using System;

namespace EODHistoricalDataDownloader.Services.Csv
{
    /// <summary>
    /// Format-neutral end-of-day bar. Writers translate this into their target shape.
    /// </summary>
    public sealed record EndOfDayRow(
        string Ticker,
        string? Name,
        DateTime Date,
        decimal? Open,
        decimal? High,
        decimal? Low,
        decimal? Close,
        decimal? AdjustedClose,
        long? Volume);
}
