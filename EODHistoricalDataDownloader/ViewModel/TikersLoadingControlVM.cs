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
        public ObservableCollection<LoadingStatus> Tickers { get; set; }

        public TikersLoadingControlVM()
        {
            Tickers = new ObservableCollection<LoadingStatus>();
        }

        public TikersLoadingControlVM(List<string> tickers)
        {
            List<LoadingStatus> ls = new();
            foreach (string ticker in tickers)
                ls.Add(new LoadingStatus(ticker));
            Tickers = new ObservableCollection<LoadingStatus>(ls);
        }

        public ICommand OpenSearchWindow
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    SearchWindow searchWindow = new();
                    SearchWindowVM searchWindowVM = new();
                    searchWindow.DataContext = searchWindowVM;
                    searchWindow.Show();
                    searchWindowVM.AddTickersEvent += SearchWindow_AddTickersEvent;
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
                LoadingStatus t = new()
                {
                    Ticker = ticker,
                    Status = "New"
                };
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
                while (!fstream?.EndOfStream ?? true)
                {
                    string text = fstream?.ReadLine() ?? "";
                    Tickers.Add(new LoadingStatus() { Ticker = text });
                }
                fstream?.Close();
            }
        }
    }
}
