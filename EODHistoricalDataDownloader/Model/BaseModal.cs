using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EODHistoricalDataDownloader.Model
{
    internal abstract class BaseModal : INotifyPropertyChanged
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
    }
}
