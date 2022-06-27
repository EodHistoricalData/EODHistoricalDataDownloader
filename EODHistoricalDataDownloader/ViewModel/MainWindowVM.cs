using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Program;
using EODHistoricalDataDownloader.Utils;
using EODHistoricalDataDownloader.View;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.ViewModel
{
    internal class MainWindowVM : BaseVM
    {
        public string AppName
        {
            get => _appName;
            set
            {
                _appName = value;
                OnPropertyChanged(nameof(AppName));
            }
        }
        public string _appName;

        #region Pages

        public Page CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
            }
        }
        private Page _currentPage;

        public Page WelcomePage
        {
            get => _welcomePage;
            set
            {
                _welcomePage = value;
                OnPropertyChanged(nameof(WelcomePage));
            }
        }
        private Page _welcomePage;

        public Page SettingsPage
        {
            get => _settingsPage;
            set
            {
                _settingsPage = value;
                OnPropertyChanged(nameof(SettingsPage));
            }
        }
        private Page _settingsPage;

        public Page IntradayPage
        {
            get => _intradayPage;
            set
            {
                _intradayPage = value;
                OnPropertyChanged(nameof(IntradayPage));
            }
        }
        private Page _intradayPage;

        public Page EndOfDayPage
        {
            get => _endOfDayPage;
            set
            {
                _endOfDayPage = value;
                OnPropertyChanged(nameof(EndOfDayPage));
            }
        }
        private Page _endOfDayPage;

        public Page FundamentalPage
        {
            get => _fundamentalPage;
            set
            {
                _fundamentalPage = value;
                OnPropertyChanged(nameof(FundamentalPage));
            }
        }
        private Page _fundamentalPage;


        public Page TaskManagerPage
        {
            get => _taskManagerPage;
            set
            {
                _taskManagerPage = value;
                OnPropertyChanged(nameof(TaskManagerPage));
            }
        }
        private Page _taskManagerPage;

        public Page AboutPage
        {
            get => _aboutPage;
            set
            {
                _aboutPage = value;
                OnPropertyChanged(nameof(AboutPage));
            }
        }
        private Page _aboutPage;

        #endregion

        public MainWindowVM()
        {
            AppName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name
                + " " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            WelcomePage WelcomePage = new(Program.Program.GetVersions());
            CurrentPage = WelcomePage;
        }

        public ICommand GoToEndOfDay
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (EndOfDayPage == null)
                    {
                        EndOfDayPage = new EndOfDayPage();
                    }
                    CurrentPage = EndOfDayPage;
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
        public ICommand GoToIntraday
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (IntradayPage == null)
                    {
                        IntradayPage = new IntradayPage();
                    }
                    CurrentPage = IntradayPage;
                },
                (obj) =>
                {
                    return true;
                });
            }
        }

        //public ICommand GoToFundamental
        //{
        //    get
        //    {
        //        return new DelegateCommand((obj) =>
        //        {
        //            if (FundamentalPage == null)
        //            {
        //                FundamentalPage = new FundamentalPage();
        //            }
        //            CurrentPage = FundamentalPage;
        //        },
        //        (obj) =>
        //        {
        //            return true;
        //        });
        //    }
        //}

        public ICommand GoToTaskManager
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (TaskManagerPage == null)
                    {
                        TaskManagerPage = new TaskManagerPage();
                    }
                    CurrentPage = TaskManagerPage;
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
        public ICommand GoToSettings
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (SettingsPage == null)
                    {
                        SettingsPage = new SettingsPage();
                    }
                    CurrentPage = SettingsPage;
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
        public ICommand GoToAbout
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    if (AboutPage == null)
                    {
                        AboutPage = new AboutPage();
                    }
                    CurrentPage = AboutPage;
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
        public ICommand Exit
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Settings.Save();
                    CloseWindow();
                },
                (obj) =>
                {
                    return true;
                });
            }
        }
    }
}