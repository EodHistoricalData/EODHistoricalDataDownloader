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

                int processorCount = Environment.ProcessorCount;
                if (_maxThreads > 2 * processorCount) _maxThreads = 2 * processorCount;
                if (_maxThreads < 1) _maxThreads = 1;

                OnPropertyChanged(nameof(MaxThreads));
            }
        }
        private int _maxThreads = Settings.SettingsFields.MaxThreads;

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
