using CsvHelper;
using CsvHelper.Configuration;
using SpeedUpImportingProcess.BenchMark.Extensions.DTO;
using SpeedUpImportingProcess.BenchMark.Extensions.Enumeration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Extensions.CSV
{
    public static class CsvExtensions
    {
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="propertyCustomConversion">propertyName, Value => returnedValue</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateLines<T>(this string filePath, Func<string, string, string> propertyCustomConversion, CancellationToken cancellationToken) where T : class
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = DetectDelimiter(filePath),
                //TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null
            };
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, csvConfig);

            var stringProps = typeof(T).GetProperties().Where(c => c.PropertyType == typeof(string)).ToArray();

            foreach (var item in csv.GetRecords<T>())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                stringProps.ForAll(property =>
                {
                    var value = property.GetValue(item);
                    if (value is string stringValue && !String.IsNullOrEmpty(stringValue))
                    {
                        var trimmed = stringValue.Trim();

                        if (propertyCustomConversion != null)
                        {
                            trimmed = propertyCustomConversion(property.Name, trimmed);
                        }
                        if (trimmed != stringValue)
                        {
                            property.SetValue(item, trimmed);
                        }
                    }
                }, cancellationToken);
                yield return item;
            }
        }

        public static IEnumerable<T> EnumerateLines<T>(this string filePath, Func<string, string> propertyCustomConversion, CancellationToken cancellationToken) where T : class
        {
            return filePath.EnumerateLines<T>((propertyName, value) => value, cancellationToken);
        }

        public static IEnumerable<T> EnumerateLines<T>(this string filePath, CancellationToken cancellationToken) where T : class
        {
            return filePath.EnumerateLines<T>((x) => x, cancellationToken);
        }
        public static T[] ReadAllLines<T>(this string filePath, CancellationToken cancellationToken) where T : class
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = DetectDelimiter(filePath),
                //TrimOptions = TrimOptions.Trim,
                MissingFieldFound = null
            };
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, csvConfig);

            return csv.GetRecords<T>().ToArray();

        }
        public static IEnumerable<RowDataDto> EnumerateLines(this FileDataDto fileImport, CancellationToken cancellationToken)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = DetectDelimiter(fileImport.FileFullName),
                MissingFieldFound = null,
                BadDataFound = context =>
                {
                }
            };
            using (var reader = new StreamReader(fileImport.FileFullName))
            {

                using (var csv = new CsvReader(reader, csvConfig))
                {
                    csv.Read();
                    csv.ReadHeader();

                    int index = 0;

                    while (csv.Read())
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }
                        var query = Enumerable.Range(0, count: csv.HeaderRecord.Length)
                             .Select(columnIndex => new FieldDataDto()
                             {
                                 ColumnIndex = columnIndex,
                                 Value = csv.GetField(columnIndex),
                             });

                        yield return new RowDataDto()
                        {
                            Index = Interlocked.Increment(ref index),
                            Fields = query.ToArray()
                        };
                    }
                }
            }
        }

        private static string DetectDelimiter(string filePath)
        {
            using StreamReader reader = new(filePath);
            return (reader.ReadLine() is string firstLine && firstLine.Contains(",")) ? "," : "\t";
        }

        private static object lockGetHeader = new object();

        public static string[] GetHeaderColumns(this string filePath, CancellationToken cancellationToken)
        {
            lock (lockGetHeader)
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = DetectDelimiter(filePath),
                    MissingFieldFound = null
                };
                using (var reader = new StreamReader(filePath))
                {
                    using (var csv = new CsvReader(reader, csvConfig))
                    {
                        csv.Read();
                        csv.ReadHeader();
                        return csv.HeaderRecord;
                    }
                }
            }
        }

        public static IEnumerable<string> ReadHeaders(this FileDataDto fileImport, CancellationToken cancellationToken)
        {
            var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = DetectDelimiter(fileImport.FileFullName),
                MissingFieldFound = null,
                BadDataFound = context =>
                {
                }
            };
            using (var reader = new StreamReader(fileImport.FileFullName))
            {

                using (var csv = new CsvReader(reader, csvConfig))
                {
                    csv.Read();
                    csv.ReadHeader();
                    csv.Read();

                    return csv.HeaderRecord;
                }
            }
        }

        public static void WriteCSV(this DataTable data, string path, CancellationToken stoppingToken)
        {
            using (var writer = new StreamWriter(path))
            {
                using (CsvWriter csv = new(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "\t",
                }))
                {
                    foreach (DataColumn dc in data.Columns)
                    {
                        csv.WriteField(dc.ColumnName);
                    }
                    csv.NextRecord();

                    foreach (DataRow dr in data.Rows)
                    {
                        foreach (DataColumn dc in data.Columns)
                        {
                            csv.WriteField(dr[dc]);
                        }
                        csv.NextRecord();
                    }
                    writer.Flush();
                }
            }
        }
    }
}
