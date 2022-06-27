using System;
using System.Windows;
using System.Windows.Controls;

namespace EODHistoricalDataDownloader.View
{
    /// <summary>
    /// Логика взаимодействия для IntradayHistoricalData.xaml
    /// </summary>
    public partial class IntradayPage : Page
    {
        public IntradayPage()
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
                    switch (cbxInterval.SelectedIndex)
                    {
                        case 0:
                            {
                                dtFrom.SelectedDate = DateTime.Today.AddDays(-120);
                                break;
                            }
                        case 1:
                            {
                                dtFrom.SelectedDate = DateTime.Today.AddDays(-600);
                                break;
                            }
                        case 2:
                            {
                                dtFrom.SelectedDate = DateTime.Today.AddDays(-7200);
                                break;
                            }
                    }
                    lFrom.IsEnabled = false;
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
