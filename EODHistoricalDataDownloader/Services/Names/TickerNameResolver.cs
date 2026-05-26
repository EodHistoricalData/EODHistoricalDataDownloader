using EOD;

using EODHistoricalDataDownloader.Services.Csv;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace EODHistoricalDataDownloader.Services.Names
{
    /// <summary>
    /// Lazy-fetch + persistent-cache resolver. On the first export of a never-before-seen
    /// ticker, calls /api/search and caches the result to %AppData%/EODHistoricalData/
    /// EODHistoricalDataDownloader/name-cache.json. Subsequent calls hit memory.
    /// </summary>
    public sealed class TickerNameResolver : ITickerNameResolver
    {
        private readonly ConcurrentDictionary<string, string> _memory = new(StringComparer.OrdinalIgnoreCase);
        private readonly SemaphoreSlim _diskGate = new(1, 1);
        private readonly string _cachePath;
        private readonly Func<API> _apiFactory;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public TickerNameResolver(string apiKey, WebProxy? proxy, string userFolder, string programName)
        {
            _cachePath = Path.Combine(userFolder, "name-cache.json");
            _apiFactory = () => new API(apiKey, proxy, programName);
            LoadFromDisk();
        }

        public async Task<string> ResolveAsync(string ticker, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(ticker))
                return string.Empty;

            if (_memory.TryGetValue(ticker, out var cached) && !string.IsNullOrEmpty(cached))
                return cached;

            string fallback = SymbolUtils.StripExchange(ticker);

            try
            {
                var api = _apiFactory();
                var results = await api.GetSearchResultAsync(ticker);
                ct.ThrowIfCancellationRequested();

                string? name = null;
                if (results != null && results.Count > 0)
                {
                    var stripped = SymbolUtils.StripExchange(ticker);
                    var match = results.FirstOrDefault(r =>
                        string.Equals(r.Code, ticker, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(r.Code, stripped, StringComparison.OrdinalIgnoreCase));
                    name = (match ?? results[0]).Name;
                }

                if (string.IsNullOrWhiteSpace(name))
                    name = fallback;

                _memory[ticker] = name!;
                await PersistAsync(ct).ConfigureAwait(false);
                return name!;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TickerNameResolver: failed to resolve '{ticker}': {ex.Message}");
                _memory[ticker] = fallback;
                return fallback;
            }
        }

        private void LoadFromDisk()
        {
            try
            {
                if (!File.Exists(_cachePath)) return;
                var json = File.ReadAllText(_cachePath);
                var map = JsonSerializer.Deserialize<Dictionary<string, string>>(json, _jsonOptions);
                if (map == null) return;
                foreach (var kvp in map)
                    _memory[kvp.Key] = kvp.Value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TickerNameResolver: cache read failed: {ex.Message}");
            }
        }

        private async Task PersistAsync(CancellationToken ct)
        {
            await _diskGate.WaitAsync(ct).ConfigureAwait(false);
            try
            {
                var dir = Path.GetDirectoryName(_cachePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var snapshot = new Dictionary<string, string>(_memory, StringComparer.OrdinalIgnoreCase);
                var tmp = _cachePath + ".tmp";
                await File.WriteAllTextAsync(tmp, JsonSerializer.Serialize(snapshot, _jsonOptions), ct).ConfigureAwait(false);
                File.Move(tmp, _cachePath, overwrite: true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"TickerNameResolver: cache write failed: {ex.Message}");
            }
            finally
            {
                _diskGate.Release();
            }
        }
    }
}
