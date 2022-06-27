using EOD;
using EOD.Model;

using EODHistoricalDataDownloader.Utils;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.ViewModel
{
    internal class SearchWindowVM : BaseVM
    {
        public delegate void AddTickersHandler(List<string> searchResults);
        public event AddTickersHandler AddTickersEvent;

        public ObservableCollection<Model.SearchResult> SearchResults { get; set; }

        public string SearchString
        {
            get
            {
                return _searchString;
            }
            set
            {
                _searchString = value;
                GetTickers().ContinueWith(x => { });
                OnPropertyChanged(nameof(SearchResults));
            }
        }
        string _searchString = "";

        public SearchWindowVM()
        {
            SearchResults = new ObservableCollection<Model.SearchResult>();
        }

        public ICommand AddTickers
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    List<Model.SearchResult> selectedResults = SearchResults.ToList().Where(x => x.Selected).ToList();
                    List<string> selectedTickers = new();
                    foreach (Model.SearchResult ticker in selectedResults)
                    {
                        selectedTickers.Add($"{ticker.Code}.{ticker.Exchange}");
                    }

                    AddTickersEvent?.Invoke(selectedTickers);
                    CloseWindow();
                },
                (obj) =>
                {
                    return true;
                });
            }
        }

        private async Task GetTickers()
        {
            if (!string.IsNullOrWhiteSpace(SearchString))
            {
                API _api = new(AppSettings.TestApiKey, null);
                List<SearchResult> results = await _api.GetSearchResultAsync(SearchString);
                SearchResults.Clear();
                foreach (SearchResult result in results)
                {
                    SearchResults.Add(new Model.SearchResult(result));
                }
            }
        }
    }
}
