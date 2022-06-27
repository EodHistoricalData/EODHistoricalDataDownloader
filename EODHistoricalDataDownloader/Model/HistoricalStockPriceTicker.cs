using EOD.Model;

namespace EODHistoricalDataDownloader.Model
{
    internal class HistoricalStockPriceTicker : HistoricalStockPrice
    {
        public string Ticker { get; set; } = string.Empty;

        internal HistoricalStockPriceTicker(string ticker, HistoricalStockPrice historicalStockPrice)
        {
            Ticker = ticker;
            Date = historicalStockPrice.Date;
            Open = historicalStockPrice.Open;
            High = historicalStockPrice.High;
            Low = historicalStockPrice.Low;
            Close = historicalStockPrice.Close;
            Adjusted_close = historicalStockPrice.Adjusted_close;
            Volume = historicalStockPrice.Volume;
        }
    }
}
