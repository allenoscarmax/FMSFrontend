using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;

namespace FMSFrontend.Resources
{
    public static class CsvStringResourceLoader
    {
        public static void Populate(ResourceDictionary dictionary, string languageColumn)
        {
            var csvPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Language", "Strings.csv");
            if (!File.Exists(csvPath))
            {
                return;
            }

            using var reader = new StreamReader(csvPath, Encoding.UTF8, true);
            var headerLine = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return;
            }

            var headerValues = SplitCsvLine(headerLine);
            var keyIndex = Array.FindIndex(headerValues, value => string.Equals(value, "x:Key", StringComparison.OrdinalIgnoreCase));
            var languageIndex = Array.FindIndex(headerValues, value => string.Equals(value, languageColumn, StringComparison.OrdinalIgnoreCase));

            if (keyIndex < 0 || languageIndex < 0)
            {
                return;
            }

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var values = SplitCsvLine(line);
                if (values.Length <= Math.Max(keyIndex, languageIndex))
                {
                    continue;
                }

                var key = values[keyIndex].Trim();
                if (string.IsNullOrWhiteSpace(key))
                {
                    continue;
                }

                var text = values[languageIndex].Trim();
                dictionary[key] = text;
            }
        }

        private static string[] SplitCsvLine(string line)
        {
            var values = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var c = line[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            values.Add(current.ToString());
            return values.ToArray();
        }
    }
}
