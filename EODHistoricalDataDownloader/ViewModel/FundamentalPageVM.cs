namespace EODHistoricalDataDownloader.ViewModel
{
    internal class FundamentalPageVM : BaseVM
    {

        public TikersLoadingControlVM TikersLoadingControlVM { get; set; }
        public FundamentalPageVM()
        {
            TikersLoadingControlVM = new TikersLoadingControlVM();
        }


    }
}
