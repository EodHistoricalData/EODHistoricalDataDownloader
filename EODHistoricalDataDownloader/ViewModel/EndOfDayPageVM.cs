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
    internal class EndOfDayPageVM : BaseVM
    {
        public TikersLoadingControlVM TikersLoadingControlVM { get; set; }

        readonly CancellationTokenSource source = new();

        public static List<string> ListOfPeriod { get; set; } = new() { "Daily", "Weekly", "Monthly" };

        public string? Period
        {
            get => _period;
            set
            {
                _period = value;
                OnPropertyChanged(nameof(Period));
                Settings.SettingsFields.EndOfDayPeriod = Period;
                Settings.Save();
            }
        }
        private string? _period = Settings.SettingsFields.EndOfDayPeriod;

        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
                Settings.SettingsFields.EndOfDayFrom = DateFrom;
                Settings.Save();
            }
        }
        private DateTime _dateFrom = Settings.SettingsFields.EndOfDayFrom;

        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                OnPropertyChanged(nameof(DateTo));
                Settings.SettingsFields.EndOfDayTo = DateTo;
                Settings.Save();
            }
        }
        private DateTime _dateTo = Settings.SettingsFields.EndOfDayTo;

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
                Settings.SettingsFields.EndOfDayFilePath = FilePath;
                Settings.Save();
            }
        }
        private string _filePath = Settings.SettingsFields.EndOfDayFilePath;

        /// <summary>
        /// save tickers in one file
        /// </summary>
        public bool OneFile { get; set; }

        public bool IsUpdate
        {
            get => _isUpdate;
            set
            {
                _isUpdate = value;
                OnPropertyChanged(nameof(IsUpdate));
                Settings.SettingsFields.EndOfDayIsUpdate = IsUpdate;
                Settings.Save();
            }
        }
        private bool _isUpdate = Settings.SettingsFields.EndOfDayIsUpdate;

        public bool AllAvailable
        {
            get => _allAvailable;
            set
            {
                _allAvailable = value;
                OnPropertyChanged(nameof(AllAvailable));
                Settings.SettingsFields.EndOfDayAllAvailable = AllAvailable;
                Settings.Save();
            }
        }
        private bool _allAvailable = Settings.SettingsFields.EndOfDayAllAvailable;

        public WebProxy Proxy
        {
            get => _proxy;
            set
            {
                _proxy = value;
            }
        }
        private WebProxy _proxy;

        public EndOfDayPageVM()
        {
            if (Settings.SettingsFields.EndOfDayTickers == null)
            {
                TikersLoadingControlVM = new TikersLoadingControlVM();
            }
            else
            {
                TikersLoadingControlVM = new TikersLoadingControlVM(Settings.SettingsFields.EndOfDayTickers);
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
                    if (!ValidateStart())
                    {
                        return;
                    }

                    List<string> listOfTickers = new();
                    foreach (var loadingStatus in TikersLoadingControlVM.Tickers)
                    {
                        listOfTickers.Add(loadingStatus.Ticker);
                        loadingStatus.Status = "Waiting";
                        loadingStatus.Filename = "";
                    }
                    Settings.SettingsFields.EndOfDayTickers = listOfTickers;
                    Settings.Save();

                    string apiKey = Settings.SettingsFields.APIKey;
                    List<LoadingStatus> loadingStatuses = TikersLoadingControlVM.Tickers.ToList();
                    HistoricalPeriod historicalPeriod = Period switch
                    {
                        "Daily" => HistoricalPeriod.Daily,
                        "Weekly" => HistoricalPeriod.Weekly,
                        "Monthly" => HistoricalPeriod.Monthly,
                        _ => HistoricalPeriod.Daily
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
                    var loader = new EndOfDayLoader(apiKey, loadingStatuses, historicalPeriod, dateFrom, dateTo, maxThreads, proxy, isUpdate, oneFile);
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
