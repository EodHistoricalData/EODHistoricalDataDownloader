using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Program;
using EODHistoricalDataDownloader.Utils;

using Microsoft.WindowsAPICodePack.Dialogs;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.ViewModel
{
    internal class DownloadGroupVM : BaseVM
    {
        private readonly DownloadGroup _model;

        public static List<string> ListOfPeriod { get; } = new() { "Daily", "Weekly", "Monthly" };
        public static List<string> ListOfFormat { get; } = new() { "Metastock", "Amibroker" };
        public static List<string> ListOfOutput { get; } = new() { "All in one file", "Separate files" };

        public event Action<DownloadGroupVM>? DeleteRequested;

        public TikersLoadingControlVM TikersLoadingControlVM { get; set; }

        public string Id => _model.Id;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
                _model.Name = value;
                Settings.SaveDebounced();
            }
        }
        private string _name;

        public string? Period
        {
            get => _period;
            set
            {
                _period = value;
                OnPropertyChanged(nameof(Period));
                _model.Period = value ?? "Daily";
                Settings.SaveDebounced();
            }
        }
        private string? _period;

        public string? Format
        {
            get => _format;
            set
            {
                _format = value;
                OnPropertyChanged(nameof(Format));
                _model.Format = value ?? "Metastock";
                Settings.SaveDebounced();
            }
        }
        private string? _format;

        public string? Output
        {
            get => _output;
            set
            {
                _output = value;
                OnPropertyChanged(nameof(Output));
                _model.Output = value ?? "Separate files";
                Settings.SaveDebounced();
            }
        }
        private string? _output;

        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                _dateFrom = value;
                OnPropertyChanged(nameof(DateFrom));
                _model.DateFrom = value;
                Settings.SaveDebounced();
            }
        }
        private DateTime _dateFrom;

        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                _dateTo = value;
                OnPropertyChanged(nameof(DateTo));
                _model.DateTo = value;
                Settings.SaveDebounced();
            }
        }
        private DateTime _dateTo;

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
                _model.FilePath = value;
                Settings.SaveDebounced();
            }
        }
        private string _filePath;

        public bool IsUpdate
        {
            get => _isUpdate;
            set
            {
                _isUpdate = value;
                OnPropertyChanged(nameof(IsUpdate));
                _model.IsUpdate = value;
                Settings.SaveDebounced();
            }
        }
        private bool _isUpdate;

        private string _groupStatus = "";
        public string GroupStatus
        {
            get => _groupStatus;
            set { _groupStatus = value; OnPropertyChanged(nameof(GroupStatus)); }
        }

        public int TickerCount => TikersLoadingControlVM.Tickers.Count;

        public DownloadGroupVM(DownloadGroup model)
        {
            _model = model;
            _name = model.Name;
            _period = model.Period;
            _format = string.IsNullOrEmpty(model.Format) ? "Metastock" : model.Format;
            _output = model.Output;
            _dateFrom = model.DateFrom;
            _dateTo = model.DateTo;
            _filePath = model.FilePath;
            _isUpdate = model.IsUpdate;

            TikersLoadingControlVM = model.Tickers.Count > 0
                ? new TikersLoadingControlVM(model.Tickers)
                : new TikersLoadingControlVM();

            TikersLoadingControlVM.Tickers.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TickerCount));
        }

        public DownloadGroup GetModel() => _model;

        /// <summary>
        /// Sync ObservableCollection tickers back to the model's List before save/download
        /// </summary>
        public void SyncTickersToModel()
        {
            _model.Tickers = TikersLoadingControlVM.Tickers.Select(t => t.Ticker).ToList();
        }

        public ICommand SelectFilePath => new DelegateCommand((obj) =>
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
        });

        public ICommand DeleteGroup => new DelegateCommand((obj) =>
        {
            DeleteRequested?.Invoke(this);
        });
    }
}
