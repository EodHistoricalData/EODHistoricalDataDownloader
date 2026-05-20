using System;

namespace EODLoader.Services.Utils
{
    public interface ICsvHistoryService
    {
        DateTime? GetLastDate(string csvPath);
    }
}
