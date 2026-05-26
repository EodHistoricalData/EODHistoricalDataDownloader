using System;
using System.Collections.Generic;
using System.Globalization;

namespace EODHistoricalDataDownloader.Services.Csv
{
    /// <summary>
    /// Shared helpers used by MetaStock and AmiBroker writers: culture (en-US for
    /// dot-decimal output) and the canonical OHLC adjustment pass.
    /// </summary>
    public abstract class CsvWriterBase
    {
        protected static readonly CultureInfo OutputCulture = CultureInfo.GetCultureInfo("en-US");

        /// <summary>
        /// Canonical "adjusted bar" transform.
        /// factor = AdjustedClose / Close; scale O/H/L by factor and use AdjustedClose as Close.
        /// Volume is left unchanged — EOD already returns split-adjusted volume.
        /// Rows with non-positive Close (e.g. missing days) are returned unchanged to avoid
        /// divide-by-zero distortion; callers may filter them upstream if desired.
        /// </summary>
        protected static IEnumerable<EndOfDayRow> ApplyAdjustment(IEnumerable<EndOfDayRow> rows, bool adjusted)
        {
            if (!adjusted) { foreach (var r in rows) yield return r; yield break; }

            foreach (var row in rows)
            {
                if (row.Close is null || row.AdjustedClose is null || row.Close <= 0m)
                {
                    yield return row;
                    continue;
                }

                decimal factor = row.AdjustedClose.Value / row.Close.Value;
                yield return row with
                {
                    Open = row.Open * factor,
                    High = row.High * factor,
                    Low = row.Low * factor,
                    Close = row.AdjustedClose
                };
            }
        }

        protected static string FormatDecimal(decimal? value)
            => value.HasValue
                ? Math.Round(value.Value, 8, MidpointRounding.ToEven).ToString("0.########", OutputCulture)
                : string.Empty;

        protected static string FormatLong(long? value)
            => value.HasValue ? value.Value.ToString(OutputCulture) : string.Empty;
    }
}
