using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.View;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace EODHistoricalDataDownloader
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            MainWindow = new MainWindow();
            MainWindow.Show();

            Current.Dispatcher.BeginInvoke(async () =>
            {
                await Task.Delay(3000);
                bool needUpdate = await Program.Program.DoYouWantUpdate();

                if (needUpdate)
                {
                    List<Version> versions = Program.Program.GetVersionNews();
                    FormUpdateList view = new(versions);
                    view.Show();
                }
            });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}
