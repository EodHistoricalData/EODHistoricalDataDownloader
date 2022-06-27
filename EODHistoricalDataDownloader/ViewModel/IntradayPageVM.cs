using EODHistoricalDataDownloader.Commands;
using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Program;
using EODHistoricalDataDownloader.Utils;

using Microsoft.WindowsAPICodePack.Dialogs;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using static EOD.API;

namespace EODHistoricalDataDownloader.ViewModel
{
    internal class IntradayPageVM : BaseVM
    {
        public TikersLoadingControlVM TikersLoadingControlVM { get; set; }

        readonly CancellationTokenSource source = new();

        public static List<string> ListOfInterval { get; set; } = new() { "1 minute", "5 minutes", "1 hour" };

        public string Interval
        {
            get => _interval;
            set
            {
                _interval = value;
                OnPropertyChanged(nameof(Interval));
                Settings.SettingsFields.IntradayInterval = Interval;
                Settings.Save();
            }
        }
        private string _interval = Settings.SettingsFields.IntradayInterval;

        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
                Settings.SettingsFields.IntradayFrom = DateFrom;
                Settings.Save();
            }
        }
        DateTime _dateFrom = Settings.SettingsFields.IntradayFrom;

        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                OnPropertyChanged(nameof(DateTo));
                Settings.SettingsFields.IntradayTo = DateTo;
                Settings.Save();
            }
        }
        DateTime _dateTo = Settings.SettingsFields.IntradayTo;

        /// <summary>
        /// file save directory
        /// </summary>
        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
                Settings.SettingsFields.IntradayFilePath = FilePath;
                Settings.Save();
            }
        }
        private string _filePath = Settings.SettingsFields.IntradayFilePath;

        public bool IsUpdate
        {
            get => _isUpdate;
            set
            {
                _isUpdate = value;
                OnPropertyChanged(nameof(IsUpdate));
                Settings.SettingsFields.IntradayIsUpdate = IsUpdate;
                Settings.Save();
            }
        }
        private bool _isUpdate = Settings.SettingsFields.IntradayIsUpdate;

        public bool OneFile { get; set; }

        public bool AllAvailable
        {
            get => _allAvailable;
            set
            {
                _allAvailable = value;
                OnPropertyChanged(nameof(AllAvailable));
                Settings.SettingsFields.IntradayAllAvailable = AllAvailable;
                Settings.Save();
            }
        }
        private bool _allAvailable = Settings.SettingsFields.IntradayAllAvailable;

        public WebProxy Proxy
        {
            get => _proxy;
            set
            {
                _proxy = value;
            }
        }
        private WebProxy _proxy;

        public IntradayPageVM()
        {
            if (Settings.SettingsFields.IntradayTickers == null)
            {
                TikersLoadingControlVM = new TikersLoadingControlVM();
            }
            else
            {
                TikersLoadingControlVM = new TikersLoadingControlVM(Settings.SettingsFields.IntradayTickers);
            }
        }

        /// <summary>
        /// Selecting a directory to save the file to
        /// </summary>
        public ICommand SelectFilePath
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    CommonOpenFileDialog dialog = new()
                    {
                        InitialDirectory = FilePath,
                        IsFolderPicker = true
                    };
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        FilePath = dialog.FileName;
                    }
                },
                (obj) =>
                {
                    return true;
                });
            }
        }

        /// <summary>
        /// Load data and save to Csv file (button click)
        /// </summary>
        public ICommand LoadToCsvFiles
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    List<string> listOfTickers = new();
                    foreach (var loadingStatus in TikersLoadingControlVM.Tickers)
                    {
                        listOfTickers.Add(loadingStatus.Ticker);
                        loadingStatus.Status = "Waiting";
                        loadingStatus.Filename = "";
                    }
                    Settings.SettingsFields.IntradayTickers = listOfTickers;
                    Settings.Save();

                    if (!ValidateStart())
                    {
                        return;
                    }
                    string apiKey = Settings.SettingsFields.APIKey;
                    List<LoadingStatus> loadingStatuses = TikersLoadingControlVM.Tickers.ToList();
                    IntradayHistoricalInterval interval = Interval switch
                    {
                        "1 minute" => IntradayHistoricalInterval.OneMinute,
                        "5 minutes" => IntradayHistoricalInterval.FiveMinutes,
                        "1 hour" => IntradayHistoricalInterval.OneHour,
                        _ => IntradayHistoricalInterval.FiveMinutes
                    };
                    DateTime dateFrom = DateFrom;
                    DateTime dateTo = DateTo;
                    string filePath = FilePath;
                    int maxThreads = Settings.SettingsFields.MaxThreads;
                    if (maxThreads > Environment.ProcessorCount * 2) maxThreads = Environment.ProcessorCount * 2;
                    if (Settings.SettingsFields.UseProxy)
                        try
                        {
                            Proxy = new(Settings.SettingsFields.ProxyHost);
                            if (Settings.SettingsFields.WithCredentials)
                            {
                                Proxy.Credentials = new NetworkCredential(Settings.SettingsFields.ProxyUsername, Settings.SettingsFields.ProxyPassword);
                            }
                        }
                        catch (Exception)
                        {

                        }
                    var proxy = Proxy;
                    bool isUpdate = IsUpdate;
                    bool oneFile = OneFile;
                    var loader = new IntradayLoader(apiKey, loadingStatuses, interval, dateFrom, dateTo, maxThreads, proxy, isUpdate, oneFile);
                    Task.Run(() => loader.LoadToCsv(filePath, source), source.Token);
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
        /// <summary>
        /// Canceling data loading
        /// </summary>
        public ICommand CancelSavingFiles
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    source.Cancel();
                },
                (obj) =>
                {
                    return true;
                });
            }
        }

        /// <summary>
        /// Checking download directory field and tiker list
        /// </summary>
        /// <returns>False when download directory is empty or do not exist or ticker list is empty</returns>
        private bool ValidateStart()
        {
            if (FilePath == string.Empty)
            {
                MessageBox.Show("The download directory field is empty", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Directory.Exists(FilePath))
            {
                MessageBox.Show("Selected download directory do not exist", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (TikersLoadingControlVM.Tickers.Count == 0)
            {
                MessageBox.Show("The ticker list is empty", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}