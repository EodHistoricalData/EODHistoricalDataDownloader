using EODHistoricalDataDownloader.Commands;
using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Program;
using EODHistoricalDataDownloader.Services.Csv;
using EODHistoricalDataDownloader.Services.Names;
using EODHistoricalDataDownloader.Utils;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private CancellationTokenSource _cts = new();

        public ObservableCollection<DownloadGroupVM> Groups { get; set; } = new();

        private DownloadGroupVM? _selectedGroup;
        public DownloadGroupVM? SelectedGroup
        {
            get => _selectedGroup;
            set { _selectedGroup = value; OnPropertyChanged(nameof(SelectedGroup)); }
        }

        public EndOfDayPageVM()
        {
            var groups = Settings.SettingsFields.EndOfDayGroups;
            if (groups != null && groups.Count > 0)
            {
                foreach (var model in groups)
                {
                    var vm = CreateGroupVM(model);
                    Groups.Add(vm);
                }
            }
            else
            {
                var defaultGroup = new DownloadGroup { Name = "Default" };
                Settings.SettingsFields.EndOfDayGroups = new List<DownloadGroup> { defaultGroup };
                var vm = CreateGroupVM(defaultGroup);
                Groups.Add(vm);
            }

            SelectedGroup = Groups.FirstOrDefault();
        }

        private DownloadGroupVM CreateGroupVM(DownloadGroup model)
        {
            var vm = new DownloadGroupVM(model);
            vm.DeleteRequested += OnDeleteGroupRequested;
            return vm;
        }

        private void OnDeleteGroupRequested(DownloadGroupVM group)
        {
            if (Groups.Count <= 1)
            {
                MessageBox.Show("At least one group must exist.", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            group.DeleteRequested -= OnDeleteGroupRequested;
            Groups.Remove(group);
            Settings.SettingsFields.EndOfDayGroups?.Remove(group.GetModel());

            if (SelectedGroup == group)
                SelectedGroup = Groups.FirstOrDefault();

            Settings.SaveDebounced();
        }

        public ICommand AddGroup => new DelegateCommand((obj) =>
        {
            var groupNumber = Groups.Count + 1;
            var model = new DownloadGroup { Name = $"Group {groupNumber}" };

            Settings.SettingsFields.EndOfDayGroups ??= new List<DownloadGroup>();
            Settings.SettingsFields.EndOfDayGroups.Add(model);

            var vm = CreateGroupVM(model);
            Groups.Add(vm);
            SelectedGroup = vm;
            Settings.SaveDebounced();
        });

        /// <summary>
        /// Download the currently selected group
        /// </summary>
        public ICommand LoadToCsvFiles => new DelegateCommand((obj) =>
        {
            if (SelectedGroup == null) return;
            if (!ValidateGroup(SelectedGroup)) return;

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            SelectedGroup.GroupStatus = "Running...";
            PrepareAndDownloadGroup(SelectedGroup, token).ContinueWith(t =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SelectedGroup.GroupStatus = t.IsFaulted ? "Error" : "Done";
                });
            });
        });

        /// <summary>
        /// Download all groups sequentially
        /// </summary>
        public ICommand LoadAllGroups => new DelegateCommand((obj) =>
        {
            var groupsToDownload = Groups.ToList();

            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            _ = Task.Run(async () =>
            {
                foreach (var group in groupsToDownload)
                {
                    if (token.IsCancellationRequested) break;

                    bool valid = false;
                    Application.Current.Dispatcher.Invoke(() => valid = ValidateGroup(group));
                    if (!valid)
                    {
                        Application.Current.Dispatcher.Invoke(() => group.GroupStatus = "Skipped");
                        continue;
                    }

                    Application.Current.Dispatcher.Invoke(() => group.GroupStatus = "Running...");

                    try
                    {
                        await PrepareAndDownloadGroup(group, token);
                        Application.Current.Dispatcher.Invoke(() => group.GroupStatus = "Done");
                    }
                    catch (OperationCanceledException)
                    {
                        Application.Current.Dispatcher.Invoke(() => group.GroupStatus = "Cancelled");
                        break;
                    }
                    catch
                    {
                        Application.Current.Dispatcher.Invoke(() => group.GroupStatus = "Error");
                    }
                }
            }, token);
        });

        public ICommand CancelSavingFiles => new DelegateCommand((obj) =>
        {
            _cts.Cancel();
        });

        private async Task PrepareAndDownloadGroup(DownloadGroupVM group, CancellationToken ct)
        {
            group.SyncTickersToModel();

            var loadingStatuses = group.TikersLoadingControlVM.Tickers.ToList();

            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var status in loadingStatuses)
                {
                    status.Status = TickerStatus.Waiting;
                    status.Filename = "";
                }
            });

            Settings.Save();

            string apiKey = Settings.SettingsFields.APIKey;
            HistoricalPeriod period = group.Period switch
            {
                "Daily" => HistoricalPeriod.Daily,
                "Weekly" => HistoricalPeriod.Weekly,
                "Monthly" => HistoricalPeriod.Monthly,
                _ => HistoricalPeriod.Daily
            };

            int maxThreads = Settings.SettingsFields.MaxThreads;
            if (maxThreads > Environment.ProcessorCount * 2)
                maxThreads = Environment.ProcessorCount * 2;

            var proxy = ProxyFactory.Create();
            bool oneFile = group.Output == "All in one file";

            CsvFormat format = CsvWriterFactory.ParseFormat(group.Format);
            ICsvWriter writer = CsvWriterFactory.Create(format);
            ITickerNameResolver names = new TickerNameResolver(
                apiKey, proxy, Program.Program.UserFolder, Program.Program.ProgramName);

            bool adjusted = group.GetModel().Adjusted;

            var loader = new EndOfDayLoader(
                apiKey, loadingStatuses, period,
                group.DateFrom, group.DateTo,
                maxThreads, proxy, group.IsUpdate, oneFile,
                format, adjusted, names, writer);

            await loader.LoadToCsvAsync(group.FilePath, ct);
        }

        private bool ValidateGroup(DownloadGroupVM group)
        {
            string prefix = Groups.Count > 1 ? $"[{group.Name}] " : "";

            if (string.IsNullOrEmpty(group.FilePath))
            {
                MessageBox.Show($"{prefix}The download directory field is empty", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (!Directory.Exists(group.FilePath))
            {
                MessageBox.Show($"{prefix}Selected download directory does not exist", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            if (group.TikersLoadingControlVM.Tickers.Count == 0)
            {
                MessageBox.Show($"{prefix}The ticker list is empty", "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
            return true;
        }
    }
}
