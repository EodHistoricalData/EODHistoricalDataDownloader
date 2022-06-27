using EOD.Model;

namespace EODHistoricalDataDownloader.Model
{
    internal class IntradayHistoricalStockPriceTicker : IntradayHistoricalStockPrice
    {
        public string Ticker { get; set; }
        internal IntradayHistoricalStockPriceTicker(string ticker, IntradayHistoricalStockPrice intraday)
        {
            Ticker = ticker;
            Timestamp = intraday.Timestamp;
            Gmtoffset = intraday.Gmtoffset;
            DateTime = intraday.DateTime;
            Open = intraday.Open;
            High = intraday.High;
            Low = intraday.Low;
            Close = intraday.Close;
            Volume = intraday.Volume;
        }
    }
}
