namespace EODHistoricalDataDownloader.Model
{
    /// <summary>
    ///  Tickers
    /// </summary>
    internal class SearchResult
    {
        public bool Selected { get; set; }

        public string? Code { get; set; }

        public string? Exchange { get; set; }

        public string? Name { get; set; }
        public SearchResult(EOD.Model.SearchResult searchResult)
        {
            Selected = false;
            Code = searchResult.Code;
            Exchange = searchResult.Exchange;
            Name = searchResult.Name;
        }
    }
}
