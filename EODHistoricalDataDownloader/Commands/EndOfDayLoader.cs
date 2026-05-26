using EOD;
using EOD.Model;

using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Services.Csv;
using EODHistoricalDataDownloader.Services.Names;

using EODLoader.Services.Utils;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static EOD.API;

namespace EODHistoricalDataDownloader.Commands
{
    internal class EndOfDayLoader
    {
        internal string ApiKey { get; set; }
        internal List<LoadingStatus> LoadingStatuses { get; set; }
        internal HistoricalPeriod Period { get; set; }
        internal DateTime DateFrom { get; set; }
        internal DateTime DateTo { get; set; }
        internal int MaxThreads { get; set; }
        internal WebProxy? Proxy { get; set; }
        internal bool IsUpdate { get; set; }
        internal bool OneFile { get; set; }
        internal CsvFormat Format { get; set; }
        internal bool Adjusted { get; set; }
        internal ITickerNameResolver Names { get; set; }
        internal ICsvWriter Writer { get; set; }

        internal EndOfDayLoader(string apiKey, List<LoadingStatus> loadingStatuses, HistoricalPeriod period,
            DateTime dateFrom, DateTime dateTo, int maxThreads, WebProxy? proxy,
            bool isUpdate, bool oneFile,
            CsvFormat format, bool adjusted,
            ITickerNameResolver names, ICsvWriter writer)
        {
            ApiKey = apiKey;
            LoadingStatuses = loadingStatuses;
            Period = period;
            DateFrom = dateFrom;
            DateTo = dateTo;
            MaxThreads = maxThreads;
            Proxy = proxy;
            IsUpdate = isUpdate;
            OneFile = oneFile;
            Format = format;
            Adjusted = adjusted;
            Names = names;
            Writer = writer;
        }

        internal async Task LoadToCsvAsync(string filePath, CancellationToken ct = default)
        {
            var _api = new API(ApiKey, Proxy, Program.Program.ProgramName);
            ICsvHistoryService csvHistory = new CsvHistoryService();
            using var semaphore = new SemaphoreSlim(MaxThreads);

            bool needsName = Format == CsvFormat.Metastock;

            try
            {
                if (!OneFile)
                {
                    var tasks = LoadingStatuses.Select(status => Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(ct);
                        try
                        {
                            ct.ThrowIfCancellationRequested();
                            status.Status = TickerStatus.Processing;

                            DateTime tickerDateFrom = DateFrom;
                            DateTime tickerDateTo = DateTo;
                            DateTime? lastDate = null;
                            string path = Path.Combine(filePath, $"{status.Ticker}.csv");

                            if (IsUpdate)
                            {
                                lastDate = csvHistory.GetLastDate(path);
                                if (lastDate.HasValue)
                                {
                                    tickerDateFrom = lastDate.Value;
                                    tickerDateTo = DateTime.Today;
                                }
                            }

                            List<HistoricalStockPrice>? response = await _api.GetEndOfDayHistoricalStockPriceAsync(
                                status.Ticker, tickerDateFrom, tickerDateTo, Period);

                            if (IsUpdate && lastDate.HasValue && response != null)
                            {
                                response = response.Where(r => r.Date > lastDate.Value).ToList();
                            }

                            string? name = needsName
                                ? await Names.ResolveAsync(status.Ticker, ct)
                                : null;

                            var rows = (response ?? new List<HistoricalStockPrice>())
                                .Where(p => p.Date.HasValue)
                                .Select(p => new EndOfDayRow(
                                    status.Ticker, name, p.Date!.Value,
                                    SymbolUtils.AsDecimal(p.Open),
                                    SymbolUtils.AsDecimal(p.High),
                                    SymbolUtils.AsDecimal(p.Low),
                                    SymbolUtils.AsDecimal(p.Close),
                                    SymbolUtils.AsDecimal(p.Adjusted_close),
                                    SymbolUtils.AsLong(p.Volume)));

                            await Writer.WriteEndOfDayAsync(path, rows, append: IsUpdate, adjusted: Adjusted, oneFile: false);
                            status.Status = TickerStatus.OK;
                            status.Filename = path;
                        }
                        catch (OperationCanceledException)
                        {
                            status.Status = TickerStatus.Error;
                            status.Filename = "Cancelled";
                        }
                        catch (Exception ex)
                        {
                            status.Status = TickerStatus.Error;
                            status.Filename = ex.Message;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, ct)).ToList();

                    await Task.WhenAll(tasks);
                }
                else
                {
                    var allRows = new ConcurrentBag<EndOfDayRow>();

                    var tasks = LoadingStatuses.Select(status => Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(ct);
                        try
                        {
                            ct.ThrowIfCancellationRequested();
                            status.Status = TickerStatus.Processing;
                            List<HistoricalStockPrice>? response = await _api.GetEndOfDayHistoricalStockPriceAsync(
                                status.Ticker, DateFrom, DateTo, Period);

                            string? name = needsName
                                ? await Names.ResolveAsync(status.Ticker, ct)
                                : null;

                            if (response != null)
                            {
                                foreach (var p in response)
                                {
                                    if (!p.Date.HasValue) continue;
                                    allRows.Add(new EndOfDayRow(
                                        status.Ticker, name, p.Date.Value,
                                        SymbolUtils.AsDecimal(p.Open),
                                        SymbolUtils.AsDecimal(p.High),
                                        SymbolUtils.AsDecimal(p.Low),
                                        SymbolUtils.AsDecimal(p.Close),
                                        SymbolUtils.AsDecimal(p.Adjusted_close),
                                        SymbolUtils.AsLong(p.Volume)));
                                }
                            }
                            status.Status = TickerStatus.OK;
                        }
                        catch (OperationCanceledException)
                        {
                            status.Status = TickerStatus.Error;
                            status.Filename = "Cancelled";
                        }
                        catch (Exception ex)
                        {
                            status.Status = TickerStatus.Error;
                            status.Filename = ex.Message;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, ct)).ToList();

                    await Task.WhenAll(tasks);

                    var sortedRows = allRows
                        .OrderBy(r => r.Ticker)
                        .ThenBy(r => r.Date)
                        .ToList();
                    string path = Path.Combine(filePath, "End of Day Tickers.csv");
                    await Writer.WriteEndOfDayAsync(path, sortedRows, append: IsUpdate, adjusted: Adjusted, oneFile: true);
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected — no action needed
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EndOfDayLoader error: {ex}");
                LoadingStatuses
                    .Where(s => s.Status == TickerStatus.Processing)
                    .ToList()
                    .ForEach(s => { s.Status = TickerStatus.Error; s.Filename = ex.Message; });
            }
        }
    }
}
