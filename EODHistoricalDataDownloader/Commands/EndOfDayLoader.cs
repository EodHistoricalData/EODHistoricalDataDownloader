using EOD;
using EOD.Model;

using EODHistoricalDataDownloader.Model;

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
        internal WebProxy Proxy { get; set; }
        internal bool IsUpdate { get; set; }
        internal bool OneFile { get; set; }

        internal EndOfDayLoader(string apiKey, List<LoadingStatus> loadingStatuses, HistoricalPeriod period, DateTime dateFrom, DateTime dateTo,
            int maxThreads, WebProxy proxy, bool isUpdate, bool oneFile)
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
        }

        internal async Task LoadToCsvAsync(string filePath, CancellationToken ct = default)
        {
            var _api = new API(ApiKey, Proxy, Program.Program.ProgramName);
            IUtilsService utils = new UtilsService();
            ICsvHistoryService csvHistory = new CsvHistoryService();
            using var semaphore = new SemaphoreSlim(MaxThreads);

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
                            DateTime? lastDate = null;
                            string path = Path.Combine(filePath, $"{status.Ticker}.csv");

                            if (IsUpdate)
                            {
                                lastDate = csvHistory.GetLastDate(path);
                                if (lastDate.HasValue)
                                    tickerDateFrom = lastDate.Value;
                            }

                            List<HistoricalStockPrice>? response = await _api.GetEndOfDayHistoricalStockPriceAsync(
                                status.Ticker, tickerDateFrom, DateTo, Period);

                            if (IsUpdate && lastDate.HasValue && response != null)
                            {
                                response = response.Where(r => r.Date > lastDate.Value).ToList();
                            }

                            await utils.CreateCVSFile(response, path, IsUpdate);
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
                    var responseTickerList = new ConcurrentBag<HistoricalStockPriceTicker>();

                    var tasks = LoadingStatuses.Select(status => Task.Run(async () =>
                    {
                        await semaphore.WaitAsync(ct);
                        try
                        {
                            ct.ThrowIfCancellationRequested();
                            status.Status = TickerStatus.Processing;
                            List<HistoricalStockPrice>? response = await _api.GetEndOfDayHistoricalStockPriceAsync(status.Ticker, DateFrom, DateTo, Period);
                            foreach (var item in response)
                            {
                                responseTickerList.Add(new HistoricalStockPriceTicker(status.Ticker, item));
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

                    var sortedList = responseTickerList.OrderBy(x => x.Ticker).ToList();
                    string path = Path.Combine(filePath, "End of Day Tickers.csv");
                    await utils.CreateCVSFile(sortedList, path, IsUpdate);
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
