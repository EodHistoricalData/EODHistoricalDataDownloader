using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace EODHistoricalDataDownloader.View
{
    /// <summary>
    /// Логика взаимодействия для SettingsPage.xaml
    /// </summary>
    public partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void MaxThreads_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!Int32.TryParse(e.Text, out _) && e.Text != "-")
            {
                e.Handled = true;
            }
        }

        private void MaxThreads_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
