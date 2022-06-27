using EODHistoricalDataDownloader.Model;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;

namespace EODHistoricalDataDownloader.View
{
    /// <summary>
    /// Логика взаимодействия для WelcomePage.xaml
    /// </summary>
    public partial class WelcomePage : Page
    {
        internal WelcomePage(List<Version> versions)
        {
            InitializeComponent();
            foreach (var version in versions)
            {
                textBlock.Inlines.Add(new Bold(new Run("v " + version.Text + ", " + version.Date.ToString("D"))));
                textBlock.Inlines.Add(new Run(version.Description + "\n"));
            }
        }
    }
}
