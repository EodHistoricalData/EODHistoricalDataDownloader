using System;

namespace EODHistoricalDataDownloader.Services.Csv
{
    public static class SymbolUtils
    {
        /// <summary>
        /// "AAPL.US" -> "AAPL", "LLOY.LSE" -> "LLOY", "BRK-B.US" -> "BRK-B".
        /// Splits on the LAST '.' so multi-dot tickers stay intact.
        /// </summary>
        public static string StripExchange(string ticker)
        {
            if (string.IsNullOrEmpty(ticker)) return ticker;
            int idx = ticker.LastIndexOf('.');
            return idx <= 0 ? ticker : ticker.Substring(0, idx);
        }

        /// <summary>
        /// Convert any boxed number-like value to decimal? for our row DTOs.
        /// Tolerates the EOD wrapper exposing decimal, double, float, int, or long.
        /// </summary>
        public static decimal? AsDecimal(object? value) => value switch
        {
            null => null,
            decimal d => d,
            double d => (decimal)d,
            float f => (decimal)f,
            long l => l,
            int i => i,
            _ => Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture)
        };

        public static long? AsLong(object? value) => value switch
        {
            null => null,
            long l => l,
            int i => i,
            decimal d => (long)d,
            double d => (long)d,
            float f => (long)f,
            _ => Convert.ToInt64(value, System.Globalization.CultureInfo.InvariantCulture)
        };
    }
}
