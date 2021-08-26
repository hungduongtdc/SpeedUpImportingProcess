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
    public static class ParallelImportService
    {
        public static void ImportParallel(this string inputFileFullPath, Func<TestDBContext> contextFactory)
        {
            int count = 0;
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            inputFileFullPath
                .EnumerateLines<AttendanceCSVDto>(cancellationTokenSource.Token)
                .Select((val, ind) => new { val, ind })
                .Partition(4000)
                .AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .ForAll(partition =>
                {
                    using (var context = contextFactory())
                    {
                        foreach (var item in partition)
                        {

                            if (DateTime.TryParse(item.val.AttendanceDate, out DateTime actualAttendanceDateValue))
                            {
                                context.Add(new Attendance()
                                {
                                    AttendanceCode = item.val.AttendanceCode,
                                    AttendanceDate = actualAttendanceDateValue,
                                    StudentId = item.val.StudentId
                                });
                                if (count++ == 5000)
                                {
                                    context.BulkSaveChanges();
                                    context.ChangeTracker.Clear();
                                    count = 0;
                                }
                            }
                            else
                            {
                                Console.WriteLine($"Error at line: {item.ind} with wrong datetime format");
                            }
                        }
                        context.SaveChanges();
                    }
                });
        }
    }
}
