using System;

namespace EODHistoricalDataDownloader.Model
{
    internal class Version
    {
        internal string Name;
        internal int Major;
        internal int Minor;
        internal int Build;
        internal int Revision;
        /// <summary>
        /// 
        /// </summary>
        internal string Text { get { return $"{Major}.{Minor}.{Build}.{Revision}"; } }
        internal DateTime Date;
        internal string Description;
        internal string Link;
    }
}
