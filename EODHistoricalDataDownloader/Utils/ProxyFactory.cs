using EODHistoricalDataDownloader.Program;

using System.Net;

namespace EODHistoricalDataDownloader.Utils
{
    internal static class ProxyFactory
    {
        internal static WebProxy? Create()
        {
            if (!Settings.SettingsFields.UseProxy) return null;
            var proxy = new WebProxy(Settings.SettingsFields.ProxyHost);
            if (Settings.SettingsFields.WithCredentials)
                proxy.Credentials = new NetworkCredential(
                    Settings.SettingsFields.ProxyUsername, Settings.SettingsFields.ProxyPassword);
            return proxy;
        }
    }
}
