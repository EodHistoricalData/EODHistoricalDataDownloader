using System;
using System.Windows;
using System.Windows.Controls;


namespace EODHistoricalDataDownloader.View
{
    /// <summary>
    /// Логика взаимодействия для EndOfDayPage.xaml
    /// </summary>
    public partial class EndOfDayPage : Page
    {
        public EndOfDayPage()
        {
            InitializeComponent();
            CbxAllAvailable_Click(null, null);
        }

        private void CbxAllAvailable_Click(object sender, RoutedEventArgs e)
        {
            if (cbxAllAvailable.IsChecked != null)
            {
                if (cbxAllAvailable.IsChecked.Value)
                {
                    lFrom.IsEnabled = false;
                    dtFrom.SelectedDate = new DateTime(1970, 1, 1);
                    dtFrom.IsEnabled = false;
                    lTo.IsEnabled = false;
                    dtTo.SelectedDate = DateTime.Today;
                    dtTo.IsEnabled = false;
                }
                else
                {
                    lFrom.IsEnabled = true;
                    dtFrom.IsEnabled = true;
                    lTo.IsEnabled = true;
                    dtTo.IsEnabled = true;
                }
            }
        }
    }
}
