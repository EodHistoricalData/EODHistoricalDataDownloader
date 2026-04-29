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
                    writer.WriteLine(string.Join(",", props.Select(p => EscapeCsvField(p.GetValue(item, null)))));
                }
            }
            else
            {
                using StreamWriter writer = new(path);
                writer.WriteLine(string.Join(",", props.Select(p => EscapeCsvField(p.Name))));

                foreach (var item in items)
                {
                    writer.WriteLine(string.Join(",", props.Select(p => EscapeCsvField(p.GetValue(item, null)))));
                }
            }
        }

        private static string EscapeCsvField(object? value)
        {
            if (value == null) return "";
            string field = value.ToString() ?? "";
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            return field;
        }

    }
}
