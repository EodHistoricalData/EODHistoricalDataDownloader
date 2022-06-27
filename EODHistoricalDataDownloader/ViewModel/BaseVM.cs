using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EODHistoricalDataDownloader.ViewModel
{
    public abstract class BaseVM : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Закрыть View текущего ViewModel
        /// </summary>
        public void CloseWindow()
        {
            Window?.Owner?.Activate();
            Window?.Close();
        }


        public Window? Window
        {
            get
            {
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.DataContext == this)
                    {
                        return window;
                    }
                }
                return null;
            }
        }
    }
}
