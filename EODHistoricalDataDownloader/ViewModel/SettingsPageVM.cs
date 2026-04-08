using EODHistoricalDataDownloader.Program;
using EODHistoricalDataDownloader.Utils;
using System;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.ViewModel
{
    internal class SettingsPageVM : BaseVM
    {
        public string? APIKey
        {
            get => _APIKey;
            set
            {
                _APIKey = value;
                OnPropertyChanged(nameof(APIKey));
            }
        }
        private string? _APIKey = Settings.SettingsFields.APIKey;

        public int MaxThreads
        {
            get => _maxThreads;
            set
            {
                _maxThreads = value;
                OnPropertyChanged(nameof(MaxThreads));
            }
        }
        private int _maxThreads = Settings.SettingsFields.MaxThreads > 1 ? 2 : 1;

        public bool OneThread
        {
            get => _oneThread;
            set
            {
                _oneThread = value;
                if (value)
                    MaxThreads = 1;
                OnPropertyChanged(nameof(OneThread));
            }
        }
        private bool _oneThread = Settings.SettingsFields.MaxThreads == 1;

        public bool TwoThread
        {
            get => _twoThread;
            set
            {
                _twoThread = value;
                if (value)
                    MaxThreads = 2;
                OnPropertyChanged(nameof(TwoThread));
            }
        }
        private bool _twoThread = Settings.SettingsFields.MaxThreads > 1;

        public int MaxThreadsRecommended
        {
            get => _maxThreadsRecommended;
        }
        private int _maxThreadsRecommended = Environment.ProcessorCount;

        public bool UseProxy
        {
            get => _useProxy;
            set
            {
                _useProxy = value;
                OnPropertyChanged(nameof(UseProxy));
            }
        }
        private bool _useProxy = Settings.SettingsFields.UseProxy;

        public string ProxyHost
        {
            get => _proxyHost;
            set
            {
                _proxyHost = value;
                OnPropertyChanged(nameof(ProxyHost));
            }
        }
        private string _proxyHost = Settings.SettingsFields.ProxyHost;

        public bool WithCredentials
        {
            get => _withCredentials;
            set
            {
                _withCredentials = value;
                OnPropertyChanged(nameof(WithCredentials));
            }
        }
        private bool _withCredentials = Settings.SettingsFields.WithCredentials;

        public string ProxyUsername
        {
            get => _proxyUsername;
            set
            {
                _proxyUsername = value;
                OnPropertyChanged(nameof(ProxyUsername));
            }
        }
        private string _proxyUsername = Settings.SettingsFields.ProxyUsername;

        public string ProxyPassword
        {
            get => _proxyPassword;
            set
            {
                _proxyPassword = value;
                OnPropertyChanged(nameof(ProxyPassword));

            }
        }
        private string _proxyPassword = Settings.SettingsFields.ProxyPassword;

        public SettingsPageVM()
        {

        }

        public ICommand SaveSettings
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Settings.SettingsFields.APIKey = APIKey;
                    Settings.SettingsFields.MaxThreads = MaxThreads;
                    Settings.SettingsFields.UseProxy = UseProxy;
                    Settings.SettingsFields.ProxyHost = ProxyHost;
                    Settings.SettingsFields.WithCredentials = WithCredentials;
                    Settings.SettingsFields.ProxyUsername = ProxyUsername;
                    Settings.SettingsFields.ProxyPassword = ProxyPassword;
                    Settings.Save();
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
    }
}
