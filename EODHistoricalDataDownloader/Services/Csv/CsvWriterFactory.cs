using EODHistoricalDataDownloader.Model;

using System;

namespace EODHistoricalDataDownloader.Services.Csv
{
    public static class CsvWriterFactory
    {
        public static ICsvWriter Create(CsvFormat format) => format switch
        {
            CsvFormat.Metastock => new MetaStockCsvWriter(),
            CsvFormat.Amibroker => new AmiBrokerCsvWriter(),
            CsvFormat.Default => new LegacyDefaultCsvWriter(),
            _ => new LegacyDefaultCsvWriter()
        };

        /// <summary>
        /// Tolerant parser for the legacy string-typed Format field on DownloadGroup.
        /// Anything we don't recognise falls back to Default (legacy behavior).
        /// </summary>
        public static CsvFormat ParseFormat(string? format)
        {
            if (string.IsNullOrWhiteSpace(format)) return CsvFormat.Default;
            return format.Trim().ToLowerInvariant() switch
            {
                "metastock" => CsvFormat.Metastock,
                "amibroker" => CsvFormat.Amibroker,
                _ => CsvFormat.Default
            };
        }
    }
}
