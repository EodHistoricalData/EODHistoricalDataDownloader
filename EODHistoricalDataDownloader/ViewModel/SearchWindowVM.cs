using EOD;
using EOD.Model;

using EODHistoricalDataDownloader.Program;
using EODHistoricalDataDownloader.Utils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.ViewModel
{
    internal class SearchWindowVM : BaseVM
    {
        public delegate void AddTickersHandler(List<string> searchResults);
        public event AddTickersHandler AddTickersEvent;

        public ObservableCollection<Model.SearchResult> SearchResults { get; set; }

        private CancellationTokenSource? _searchCts;
        private API? _api;
        private string _apiKeyForApi = "";

        public string SearchString
        {
            get => _searchString;
            set
            {
                _searchString = value;
                OnPropertyChanged(nameof(SearchString));
                DebounceSearch();
            }
        }
        string _searchString = "";

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

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

        private async void DebounceSearch()
        {
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                await Task.Delay(300, token);
                if (token.IsCancellationRequested) return;
                await GetTickers(token);
            }
            catch (OperationCanceledException)
            {
                // expected on debounce / window close
            }
            catch (Exception ex)
            {
                // async void poison: must not let exceptions reach the dispatcher
                StatusMessage = $"Search failed: {ex.Message}";
            }
        }

        private async Task GetTickers(CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(SearchString))
            {
                SearchResults.Clear();
                StatusMessage = "";
                return;
            }

            string apiKey = Settings.SettingsFields?.APIKey ?? "";
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                SearchResults.Clear();
                StatusMessage = "Set your APIKey in Settings to search tickers.";
                return;
            }

            if (_api == null || _apiKeyForApi != apiKey)
            {
                _api = new API(apiKey, null);
                _apiKeyForApi = apiKey;
            }

            StatusMessage = "Searching…";

            List<SearchResult> results = await _api.GetSearchResultAsync(SearchString);
            if (token.IsCancellationRequested) return;

            SearchResults.Clear();
            foreach (SearchResult result in results)
            {
                SearchResults.Add(new Model.SearchResult(result));
            }

            StatusMessage = results.Count == 0 ? "No results." : "";
        }
    }
}
