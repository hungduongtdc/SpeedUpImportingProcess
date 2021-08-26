using SpeedUpImportingProcess.BenchMark.Contexts;
using SpeedUpImportingProcess.BenchMark.Contexts.Models;
using SpeedUpImportingProcess.BenchMark.DTO;
using SpeedUpImportingProcess.BenchMark.Extensions.CSV;
using SpeedUpImportingProcess.BenchMark.Extensions.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Services
{
    public static class ParallelImportAsyncService
    {
        public static Task ImportParallelAsync(this string inputFileFullPath, Func<TestDBContext> contextFactory)
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            return inputFileFullPath
                     .EnumerateLines<AttendanceCSVDto>(cancellationTokenSource.Token)
                     .Select((val, index) =>
                     {

                         val.Index = index;
                         return val;
                     })
                     .Partition(4000)
                   .SafeParallelProcessAsync(ImportAction(contextFactory), Environment.ProcessorCount);
        }
        public static Func<AttendanceCSVDto[], Task> ImportAction(Func<TestDBContext> contextFactory)
        {

            return async partition =>
            {
                using (var context = contextFactory())
                {
                    foreach (var item in partition)
                    {

                        if (DateTime.TryParse(item.AttendanceDate, out DateTime actualAttendanceDateValue))
                        {
                            context.Add(new Attendance()
                            {
                                AttendanceCode = item.AttendanceCode,
                                AttendanceDate = actualAttendanceDateValue,
                                StudentId = item.StudentId
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Error at line: {item.Index} with wrong datetime format");
                        }
                    }
                    await context.BulkSaveChangesAsync();
                    context.ChangeTracker.Clear();
                }
            };
        }
    }
}
