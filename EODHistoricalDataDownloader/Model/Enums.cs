namespace EODHistoricalDataDownloader.Model
{
    public enum TickerStatus { New, Waiting, Processing, OK, Error }
    public enum CsvFormat { Metastock, Amibroker }
    public enum OutputMode { AllInOneFile, SeparateFiles }
}
