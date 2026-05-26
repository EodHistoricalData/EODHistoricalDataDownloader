using System.Threading;
using System.Threading.Tasks;

namespace EODHistoricalDataDownloader.Services.Names
{
    /// <summary>
    /// Resolves an EOD ticker (e.g. "AAPL.US") to its company name (e.g. "Apple Inc.").
    /// On miss or error, returns a sensible fallback (typically the stripped symbol) so
    /// the export pipeline never blocks on name resolution.
    /// </summary>
    public interface ITickerNameResolver
    {
        Task<string> ResolveAsync(string ticker, CancellationToken ct = default);
    }
}
