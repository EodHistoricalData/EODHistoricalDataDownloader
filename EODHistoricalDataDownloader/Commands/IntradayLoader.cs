using EOD;
using EOD.Model;

using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Services.Csv;
using EODHistoricalDataDownloader.Services.Names;

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
    internal class IntradayLoader
    {
        internal string ApiKey { get; set; }
        internal List<LoadingStatus> LoadingStatuses { get; set; }
        internal IntradayHistoricalInterval Interval { get; set; }
        internal DateTime DateFrom { get; set; }
        internal DateTime DateTo { get; set; }
        internal int MaxThreads { get; set; }
        internal WebProxy Proxy { get; set; }
        internal bool IsUpdate { get; set; }
        internal bool OneFile { get; set; }
        internal CsvFormat Format { get; set; }
        internal ITickerNameResolver Names { get; set; }
        internal ICsvWriter Writer { get; set; }

        internal IntradayLoader(string apiKey, List<LoadingStatus> loadingStatuses, IntradayHistoricalInterval interval,
            DateTime dateFrom, DateTime dateTo, int maxThreads, WebProxy proxy,
            bool isUpdate, bool oneFile,
            CsvFormat format,
            ITickerNameResolver names, ICsvWriter writer)
        {
            ApiKey = apiKey;
            LoadingStatuses = loadingStatuses;
            Interval = interval;
            DateFrom = dateFrom;
            DateTo = dateTo;
            MaxThreads = maxThreads;
            Proxy = proxy;
            IsUpdate = isUpdate;
            OneFile = oneFile;
            Format = format;
            Names = names;
            Writer = writer;
        }

        internal async Task LoadToCsvAsync(string filePath, CancellationToken ct = default)
        {
            var _api = new API(ApiKey, Proxy, Program.Program.ProgramName);
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
                            List<IntradayHistoricalStockPrice>? response = await _api.GetIntradayHistoricalStockPriceAsync(
                                status.Ticker, DateFrom, DateTo, Interval);

                            string? name = needsName
                                ? await Names.ResolveAsync(status.Ticker, ct)
                                : null;

                            var rows = (response ?? new List<IntradayHistoricalStockPrice>())
                                .Where(p => p.DateTime.HasValue)
                                .Select(p => new IntradayRow(
                                    status.Ticker, name, p.DateTime!.Value,
                                    SymbolUtils.AsLong(p.Timestamp),
                                    p.Gmtoffset is null ? null : (int?)System.Convert.ToInt32(p.Gmtoffset),
                                    SymbolUtils.AsDecimal(p.Open),
                                    SymbolUtils.AsDecimal(p.High),
                                    SymbolUtils.AsDecimal(p.Low),
                                    SymbolUtils.AsDecimal(p.Close),
                                    SymbolUtils.AsLong(p.Volume)));

                            string path = Path.Combine(filePath, $"{status.Ticker}.csv");
                            await Writer.WriteIntradayAsync(path, rows, append: IsUpdate, oneFile: false);
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
                    var allRows = new ConcurrentBag<IntradayRow>();

                    var tasks = LoadingStatuses.Select(status => Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(ct);
                        try
                        {
                            ct.ThrowIfCancellationRequested();
                            status.Status = TickerStatus.Processing;
                            List<IntradayHistoricalStockPrice>? response = await _api.GetIntradayHistoricalStockPriceAsync(
                                status.Ticker, DateFrom, DateTo, Interval);

                            string? name = needsName
                                ? await Names.ResolveAsync(status.Ticker, ct)
                                : null;

                            if (response != null)
                            {
                                foreach (var p in response)
                                {
                                    if (!p.DateTime.HasValue) continue;
                                    allRows.Add(new IntradayRow(
                                        status.Ticker, name, p.DateTime.Value,
                                        SymbolUtils.AsLong(p.Timestamp),
                                        p.Gmtoffset is null ? null : (int?)System.Convert.ToInt32(p.Gmtoffset),
                                        SymbolUtils.AsDecimal(p.Open),
                                        SymbolUtils.AsDecimal(p.High),
                                        SymbolUtils.AsDecimal(p.Low),
                                        SymbolUtils.AsDecimal(p.Close),
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
                        .ThenBy(r => r.DateTime)
                        .ToList();
                    string path = Path.Combine(filePath, "Intraday Tickers.csv");
                    await Writer.WriteIntradayAsync(path, sortedRows, append: IsUpdate, oneFile: true);
                }
            }
            catch (OperationCanceledException)
            {
                // Cancellation is expected — no action needed
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"IntradayLoader error: {ex}");
                LoadingStatuses
                    .Where(s => s.Status == TickerStatus.Processing)
                    .ToList()
                    .ForEach(s => { s.Status = TickerStatus.Error; s.Filename = ex.Message; });
            }
        }
    }
}
