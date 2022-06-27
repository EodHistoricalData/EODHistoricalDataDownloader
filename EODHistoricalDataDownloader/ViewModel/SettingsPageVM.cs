using EODHistoricalDataDownloader.Program;

using System;

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
                Settings.SettingsFields.APIKey = APIKey;
                Settings.Save();
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
                Settings.SettingsFields.MaxThreads = MaxThreads;
                Settings.Save();
            }
        }
        private int _maxThreads = Settings.SettingsFields.MaxThreads;

        public bool UseProxy
        {
            get => _useProxy;
            set
            {
                _useProxy = value;
                OnPropertyChanged(nameof(UseProxy));
                Settings.SettingsFields.UseProxy = UseProxy;
                Settings.Save();
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
                Settings.SettingsFields.ProxyHost = ProxyHost;
                Settings.Save();
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
                Settings.SettingsFields.WithCredentials = WithCredentials;
                Settings.Save();
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
                Settings.SettingsFields.ProxyUsername = ProxyUsername;
                Settings.Save();
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
                Settings.SettingsFields.ProxyPassword = ProxyPassword;
                Settings.Save();
            }
        }
        private string _proxyPassword = Settings.SettingsFields.ProxyPassword;

        public SettingsPageVM()
        {

        }
    }
}
