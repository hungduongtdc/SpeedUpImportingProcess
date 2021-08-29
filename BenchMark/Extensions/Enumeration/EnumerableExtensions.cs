using SpeedUpImportingProcess.BenchMark.Extensions.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SpeedUpImportingProcess.BenchMark.Extensions.Enumeration
{
    public static class EnumerableExtensions
    {
        public static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            DataTable dt = new DataTable();
            typeof(T).GetProperties().ForAll(p =>
            {
                dt.Columns.Add(p.Name, p.PropertyType);
            });
            foreach (var item in data)
            {
                var newRow = dt.NewRow();
                typeof(T).GetProperties().ForAll(p =>
                {
                    newRow[p.Name] = p.GetValue(item);
                });
                dt.Rows.Add(newRow);
            }
            return dt;
        }
        public static DataTable ToDataTable(this IEnumerable<RowDataDto> rowDataDtos)
        {
            if (rowDataDtos?.Any() == false)
            {
                return new DataTable();
            }
            DataTable dataTable = new DataTable();
            dataTable.Columns.Add("RowIndex", typeof(int));

            rowDataDtos.FirstOrDefault()?.Fields.OrderBy(c => c.ColumnIndex)
                .Select(c => c.SQLColumnName)
                .ForAll(c => dataTable.Columns.Add($"{c}", typeof(string)));
            foreach (var row in rowDataDtos)
            {
                var newRow = dataTable.NewRow();
                newRow["RowIndex"] = row.Index;
                foreach (var field in row.Fields)
                {
                    newRow[$"{field.SQLColumnName}"] = field.Value;
                }
                dataTable.Rows.Add(newRow);
            }
            return dataTable;
        }

        public static IEnumerable<T[]> Partition<T>(this IEnumerable<T> source, int partitionMaxSize = 4000)
        {
            var result = new T[partitionMaxSize];
            int counter = 0;
            foreach (var item in source)
            {
                result[counter++] = item;
                if (counter == partitionMaxSize)
                {
                    counter = 0;
                    yield return result;
                    result = new T[partitionMaxSize];
                }
            }
            if (counter > 0)
                yield return result.Skip(0).Take(counter).ToArray();
        }

        public static void ForAll<T>(this IEnumerable<T> source, Action<T> action, System.Threading.CancellationToken stoppingToken)
        {
            foreach (var item in source)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                action(item);
            }
        }

        public static void ForAll<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static void ForAll<T>(this IEnumerable<T> source, Action<T, int> action, System.Threading.CancellationToken stoppingToken)
        {
            int index = 0;
            foreach (var item in source)
            {
                if (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                action(item, index++);
            }
        }

        public static void ForAll<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int index = 0;
            foreach (var item in source)
            {
                action(item, index++);
            }
        }

        public static Task<IEnumerable<R>> SelectAsync<T, R>(this Task<IEnumerable<T>> source, Func<T, R> selector)
        {
            return source.ContinueWith(c => c.Result.Select(selector));
        }

        public static Task<R[]> ToArrayAsync<T, R>(this Task<IEnumerable<T>> source, Func<T, R> selector)
        {
            return source.ContinueWith(c => c.Result?.Select(selector)?.ToArray() ?? Array.Empty<R>());
        }

        public static Task<T[]> ToArrayAsync<T>(this Task<IEnumerable<T>> source)
        {
            return source.ContinueWith(c => c.Result?.ToArray() ?? Array.Empty<T>());
        }

        public static Task<R[]> ToArrayAsync<T, R>(this Task<T[]> source, Func<T, R> selector)
        {
            return source.ContinueWith(c => c.Result?.Select(selector)?.ToArray() ?? Array.Empty<R>());
        }

        public static async Task<ConcurrentBag<TResult>> TransformAsync<TSource, TTransformResult, TResult>(this IEnumerable<TSource> source,
            Func<TSource, Task<TTransformResult>> transformer,
            int maxDegreeOfParallelism,
            Func<TTransformResult, IEnumerable<TResult>> transformToResult
        )
        {
            var transformBlock = new TransformBlock<TSource, TTransformResult>(transformer, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            });
            var retrieveResultTask = RetrieveDataAsync(transformBlock);
            foreach (var partition in source)
            {
                while (transformBlock.InputCount >= maxDegreeOfParallelism)
                {
                    await Task.Delay(10);
                }
                transformBlock.Post(partition);
            }
            transformBlock.Complete();
            await transformBlock.Completion;

            var actualResult = await retrieveResultTask;
            return new ConcurrentBag<TResult>(actualResult.SelectMany(transformToResult));
        }

        public static async Task SafeParallelProcessAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> action, int maxDegreeOfParallelism)
        {
            var actionBlock = new ActionBlock<TSource>(action, new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            });

            foreach (var partition in source)
            {
                while (actionBlock.InputCount >= maxDegreeOfParallelism)
                {
                    await Task.Delay(10);
                }
                actionBlock.Post(partition);
            }
            actionBlock.Complete();
            await actionBlock.Completion;
        }

        private static async Task<List<TResult>> RetrieveDataAsync<TSource, TResult>(TransformBlock<TSource, TResult> source)
        {
            List<TResult> result = new();
            while (await source.OutputAvailableAsync())
            {
                if (source.TryReceiveAll(out IList<TResult> items))
                {
                    result.AddRange(items);
                }
            }
            return result;
        }
    }
}
