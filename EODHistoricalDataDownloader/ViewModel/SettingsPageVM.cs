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
                NotifySaveCanExecuteChanged();
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
                if (_useProxy == value) return;
                _useProxy = value;
                OnPropertyChanged(nameof(UseProxy));
                NotifySaveCanExecuteChanged();
            }
        }
        private bool _useProxy = Settings.SettingsFields.UseProxy;

        public string ProxyHost
        {
            get => _proxyHost;
            set
            {
                if (_proxyHost == value) return;
                _proxyHost = value;
                OnPropertyChanged(nameof(ProxyHost));
                NotifySaveCanExecuteChanged();
            }
        }
        private string _proxyHost = Settings.SettingsFields.ProxyHost;

        public bool WithCredentials
        {
            get => _withCredentials;
            set
            {
                if (_withCredentials == value) return;
                _withCredentials = value;
                OnPropertyChanged(nameof(WithCredentials));
                NotifySaveCanExecuteChanged();
            }
        }
        private bool _withCredentials = Settings.SettingsFields.WithCredentials;

        public string ProxyUsername
        {
            get => _proxyUsername;
            set
            {
                if (_proxyUsername == value) return;
                _proxyUsername = value;
                OnPropertyChanged(nameof(ProxyUsername));
                NotifySaveCanExecuteChanged();
            }
        }
        private string _proxyUsername = Settings.SettingsFields.ProxyUsername;

        public string ProxyPassword
        {
            get => _proxyPassword;
            set
            {
                if (_proxyPassword == value) return;
                _proxyPassword = value;
                OnPropertyChanged(nameof(ProxyPassword));
                NotifySaveCanExecuteChanged();
            }
        }
        private string _proxyPassword = Settings.SettingsFields.ProxyPassword;

        private int _snapshotMaxThreads;
        private bool _snapshotUseProxy;
        private string _snapshotProxyHost = "";
        private bool _snapshotWithCredentials;
        private string _snapshotProxyUsername = "";
        private string _snapshotProxyPassword = "";

        private bool IsNonApiKeyDirty =>
            _maxThreads != _snapshotMaxThreads
            || _useProxy != _snapshotUseProxy
            || _proxyHost != _snapshotProxyHost
            || _withCredentials != _snapshotWithCredentials
            || _proxyUsername != _snapshotProxyUsername
            || _proxyPassword != _snapshotProxyPassword;

        private void CaptureSnapshot()
        {
            _snapshotMaxThreads = _maxThreads;
            _snapshotUseProxy = _useProxy;
            _snapshotProxyHost = _proxyHost;
            _snapshotWithCredentials = _withCredentials;
            _snapshotProxyUsername = _proxyUsername;
            _snapshotProxyPassword = _proxyPassword;
        }

        private static void NotifySaveCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public SettingsPageVM()
        {
            CaptureSnapshot();
        }

        private DelegateCommand? _saveSettings;
        public ICommand SaveSettings => _saveSettings ??= new DelegateCommand(
            _ =>
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
                CaptureSnapshot();
                NotifySaveCanExecuteChanged();
            },
            _ => IsNonApiKeyDirty);
    }
}
