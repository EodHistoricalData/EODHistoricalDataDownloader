using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EODLoader.Services.Utils
{
    public class UtilsService : IUtilsService
    {
        public async Task CreateCVSFile<T>(IEnumerable<T> items, string path, bool isUpdate)
        {
            await Task.Run(() => WriteToFile(items, path, isUpdate));
        }

        private static void WriteToFile<T>(IEnumerable<T> items, string path, bool isUpdate)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            Type itemType = typeof(T);
            var props = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            if (isUpdate)
            {
                using StreamWriter writer = new(File.Open(path, FileMode.Append));
                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(",", props.Select(p => p.GetValue(item, null))));
                }
            }
            else
            {
                using StreamWriter writer = new(path);
                writer.WriteLine(string.Join(",", props.Select(p => p.Name)));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(",", props.Select(p => p.GetValue(item, null))));
                }
            }
        }

        public bool RewriteDateBeforeLoad(string path, ref DateTime? startDate, ref DateTime? endDate)
        {
            //Проверка существует ли файл
            try
            {
                if (CVSFileIsExist(path))
                {
                    //Проверка не пустой ли файл
                    if (new FileInfo(path).Length < 2)
                    {
                        return false;
                    }

                    if (DateTime.TryParse(File.ReadLines(path).Last().Split(',')[0], out DateTime dateParse))
                    {
                        startDate = dateParse.AddDays(1);
                    }
                    else
                    {
                        return false;
                    }

                    endDate = DateTime.Now;

                    return true;
                }
                return false;
            }
            catch
            {
                throw;
            }
        }

        //Существует ли такой CSV
        private static bool CVSFileIsExist(string path)
        {
            return File.Exists(path);
        }
    }
}
