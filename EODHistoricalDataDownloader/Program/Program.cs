using EODHistoricalDataDownloader.Model;
using EODHistoricalDataDownloader.Utils;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace EODHistoricalDataDownloader.Program
{
    public static class Program
    {
        /// <summary>
        /// Название программы
        /// </summary>
        internal const string ProgramName = "EODHistoricalDataDownloader";
        internal const string CompanyName = "EODHistoricalData";
        internal const string UrlCompany = "https://eodhistoricaldata.com";
        internal const string UrlKey = "https://eodhistoricaldata.com/cp/settings";
        internal const string UrlPrice = "https://eodhistoricaldata.com/pricing";
        internal const string UrlUpdate = "https://eodhistoricaldata.com/eodhistoricaldatadownloader-updates.xml";

        /// <summary>
        /// Папка пользователя
        /// </summary>
        public static string UserFolder => Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData), CompanyName, ProgramName);


        #region Update

        public async static Task<bool> DoYouWantUpdate()
        {

            return await Task.Factory.StartNew(() => 
            {
                if (CheckUpdate())
                {
                    if (MessageBox.Show($"Find updates.\nDo you want to update the program",
                                        ProgramName,
                                        MessageBoxButtons.YesNo,
                                        MessageBoxIcon.Information,
                                        MessageBoxDefaultButton.Button1,
                                        MessageBoxOptions.ServiceNotification) == DialogResult.Yes)
                    {
                        return true;
                    }
                }
                return false;
            });
            
        }
        private static bool CheckUpdate()
        {
            try
            {
                if (GetVersionNews()?.Count > 0) return true;
            }
            catch (System.Net.WebException ex)
            {
                throw ex;
            }

            return false;
        }
        internal static List<Version> GetVersionNews()
        {
            List<Version> versions;
            try
            {
                versions = GetVersions();
            }
            catch 
            {
                return new List<Version>();
            }

            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            List<Version> versionsNew = (from i in versions
                                         where (i.Major > ver.Major) ||
                                               (i.Major == ver.Major && i.Minor > ver.Minor) ||
                                               (i.Major == ver.Major && i.Minor == ver.Minor && i.Build > ver.Build) ||
                                               (i.Major == ver.Major && i.Minor == ver.Minor && i.Build == ver.Build && i.Revision > ver.Revision)
                                         orderby i.Major descending, i.Minor descending, i.Build descending, i.Revision descending
                                         select i).ToList();
            return versionsNew;
        }

        internal static List<Version> GetVersions()
        {
            List<Version> versions = new();

            string response;
            try
            {
                response = Response.GET(UrlUpdate, "");
            }
            catch
            {
                return versions;
            }

            XmlDocument xmlDocument = new();
            xmlDocument.LoadXml(response);
            XmlElement xRoot = xmlDocument.DocumentElement;
            XmlNodeList versionsList = xRoot.SelectNodes("Version");

            foreach (XmlNode versionNode in versionsList)
            {
                Version version = new()
                {
                    Name = versionNode.Attributes["name"].Value,
                    Description = versionNode.SelectSingleNode("Description").InnerText,
                    Link = versionNode.SelectSingleNode("Link").InnerText
                };

                string[] versplit = versionNode.SelectSingleNode("Number").InnerText.Split('.');
                version.Major = int.Parse(versplit[0]);
                version.Minor = int.Parse(versplit[1]);
                version.Build = int.Parse(versplit[2]);
                version.Revision = int.Parse(versplit[3]);
                System.DateTime.TryParse(versionNode.SelectSingleNode("Date").InnerText, out version.Date);
                versions.Add(version);
            }
            return versions;
        }
        #endregion

    }
}
