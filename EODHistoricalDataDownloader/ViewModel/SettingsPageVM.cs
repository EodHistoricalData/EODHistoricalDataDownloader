using EODHistoricalDataDownloader.Program;
using EODHistoricalDataDownloader.Utils;
using System;
using System.Windows.Input;
using System.Windows.Threading;

namespace EODHistoricalDataDownloader.ViewModel
{
    public enum ApiKeySaveState
    {
        Saved,
        Saving
    }

    internal class SettingsPageVM : BaseVM
    {
        public string? APIKey
        {
            get => _APIKey;
            set
            {
                if (_APIKey == value) return;
                _APIKey = value;
                OnPropertyChanged(nameof(APIKey));
                OnPropertyChanged(nameof(HasApiKey));
                ScheduleApiKeyAutoSave();
            }
        }
        private string? _APIKey = Settings.SettingsFields.APIKey;

        public bool HasApiKey => !string.IsNullOrEmpty(_APIKey);

        public ApiKeySaveState ApiKeyState
        {
            get => _apiKeyState;
            private set
            {
                if (_apiKeyState == value) return;
                _apiKeyState = value;
                OnPropertyChanged(nameof(ApiKeyState));
            }
        }
        private ApiKeySaveState _apiKeyState = ApiKeySaveState.Saved;

        private DispatcherTimer? _apiKeyDebounce;

        private void ScheduleApiKeyAutoSave()
        {
            ApiKeyState = ApiKeySaveState.Saving;
            _apiKeyDebounce ??= CreateApiKeyDebounceTimer();
            _apiKeyDebounce.Stop();
            _apiKeyDebounce.Start();
        }

        private DispatcherTimer CreateApiKeyDebounceTimer()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            timer.Tick += (_, _) =>
            {
                _apiKeyDebounce!.Stop();
                Settings.SettingsFields!.APIKey = _APIKey;
                Settings.Save();
                ApiKeyState = ApiKeySaveState.Saved;
            };
            return timer;
        }

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

        public bool TwoConnections
        {
            get => _twoConnections;
            set
            {
                if (_twoConnections == value) return;
                _twoConnections = value;
                MaxThreads = value ? 2 : 1;
                OnPropertyChanged(nameof(TwoConnections));
            }
        }
        private bool _twoConnections = Settings.SettingsFields.MaxThreads > 1;

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
                    _apiKeyDebounce?.Stop();
                    Settings.SettingsFields.APIKey = APIKey;
                    Settings.SettingsFields.MaxThreads = MaxThreads;
                    Settings.SettingsFields.UseProxy = UseProxy;
                    Settings.SettingsFields.ProxyHost = ProxyHost;
                    Settings.SettingsFields.WithCredentials = WithCredentials;
                    Settings.SettingsFields.ProxyUsername = ProxyUsername;
                    Settings.SettingsFields.ProxyPassword = ProxyPassword;
                    Settings.Save();
                    ApiKeyState = ApiKeySaveState.Saved;
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
    }
}
