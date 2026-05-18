using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Utils;
using EODHistoricalDataDownloader.View;

using Microsoft.Win32;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.ViewModel
{
    internal class TikersLoadingControlVM : BaseVM
    {
        public LoadingStatus Ticker { get; set; }
        public ObservableCollection<LoadingStatus> Tickers { get; set; } = new ObservableCollection<LoadingStatus>();

        public TikersLoadingControlVM()
        {
            
        }

        public TikersLoadingControlVM(List<string> tickers)
        {
            foreach (string ticker in tickers)
            {
                var loadingStatus = new LoadingStatus(ticker);
                loadingStatus.Deleted += LoadingStatus_Deleted;
                Tickers.Add(loadingStatus);
            }
        }

        private void LoadingStatus_Deleted(LoadingStatus obj)
        {
            Tickers.Remove(obj);
        }

        public ICommand OpenSearchWindow
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    SearchWindow searchWindow = new();
                    if (searchWindow.DataContext is SearchWindowVM searchWindowVM)
                    {
                        searchWindowVM.AddTickersEvent += SearchWindow_AddTickersEvent;
                    }
                    searchWindow.Show();
                },
                (obj) =>
                {
                    return true;
                });
            }
        }

        public ICommand AddFromTextFile
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    GetFromTestFile();
                },
                (obj) =>
                {
                    return true;
                });
            }
        }

        public ICommand ClearList
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Tickers.Clear();
                },
                (obj) =>
                {
                    return true;
                });
            }
        }

        private void SearchWindow_AddTickersEvent(List<string> selectedResults)
        {
            foreach (string ticker in selectedResults)
            {
                var t = new LoadingStatus(ticker) { Status = TickerStatus.New };
                t.Deleted += LoadingStatus_Deleted;
                Tickers.Add(t);
            }
        }

        private void GetFromTestFile()
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "txt files (*.txt)|*.txt"
            };

            if (openFileDialog.ShowDialog() ?? false)
            {
                string filePath = openFileDialog.FileName;

                using StreamReader fstream = new(filePath);
                while (!fstream.EndOfStream)
                {
                    string text = fstream.ReadLine() ?? "";
                    var t = new LoadingStatus(text);
                    t.Deleted += LoadingStatus_Deleted;
                    Tickers.Add(t);
                }
            }
        }
    }
}
