using EODHistoricalDataDownloader.Utils;
using System.Windows.Forms;
using System;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.Model
{
    internal class LoadingStatus : BaseModal
    {

        public string Ticker { get; set; } = "";

        public event Action<LoadingStatus>? Deleted;

        public string Status
        {
            get
            {
                if (string.IsNullOrEmpty(_status))
                {
                    _status = "";
                }
                return _status;
            }
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        private string? _status;

        public string? Filename
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged(nameof(Filename));
            }
        }
        private string? _fileName;

        public LoadingStatus()
        {

        }

        public LoadingStatus(string ticker)
        {
            Ticker = ticker;
        }

        public ICommand Delete => new DelegateCommand((obj) =>
        {
            Deleted?.Invoke(this);
        }, (obj) => true);
    }
}
