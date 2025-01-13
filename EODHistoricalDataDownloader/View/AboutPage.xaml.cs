using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Documents;

namespace EODHistoricalDataDownloader.View
{
    /// <summary>
    /// Логика взаимодействия для AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://eodhd.com") { UseShellExecute = true });
        }

        private void Hyperlink_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://eodhd.com/pricing") { UseShellExecute = true });
        }

        private void Hyperlink_Click_2(object sender, System.Windows.RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("mailto:support@eodhistoricaldata.com") { UseShellExecute = true });
        }
    }
}
