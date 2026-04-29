using System;
using System.Collections.Generic;

using Version = EODHistoricalDataDownloader.Model.Version;

namespace EODHistoricalDataDownloader.Program
{
    internal static class Changelog
    {
        internal static List<Version> GetVersions() => new()
        {
            new Version
            {
                Major = 2, Minor = 1, Build = 0, Revision = 0,
                Name = "2.1.0.0",
                Date = new DateTime(2026, 4, 29),
                Description = "\n" +
                    "- Download Groups: create multiple named ticker groups, each with its own folder and settings\n" +
                    "- Download All: one-click download for all groups sequentially\n" +
                    "- True incremental update: Update mode reads the last date from existing CSV and appends only new data\n" +
                    "- Legacy settings migration: old settings are automatically converted to a Default group\n"
            },
            new Version
            {
                Major = 2, Minor = 0, Build = 9, Revision = 0,
                Name = "2.0.9.0",
                Date = new DateTime(2026, 4, 25),
                Description = "\n" +
                    "- Code quality improvements and bug fixes\n"
            },
            new Version
            {
                Major = 2, Minor = 0, Build = 1, Revision = 0,
                Name = "2.0.1.0 beta",
                Date = new DateTime(2022, 3, 5),
                Description = "\n" +
                    "- Bug-fix\n"
            },
            new Version
            {
                Major = 2, Minor = 0, Build = 0, Revision = 0,
                Name = "2.0.0.0 beta",
                Date = new DateTime(2022, 1, 14),
                Description = "\n" +
                    "- This is the initial version of the program\n"
            }
        };
    }
}
