using Microsoft.EntityFrameworkCore;
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
    public static class SequentiallyImportReduceMemoryUsageService
    {
        
        public static void ImportSequentially_ReduceMemoryUsage(this string inputFileFullPath, TestDBContext context)
        {
            int count = 0;
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            inputFileFullPath
                .EnumerateLines<AttendanceCSVDto>(cancellationTokenSource.Token)
                .ForAll((val, index) =>
                {
#if DEBUG
                    if (index % 50000 == 0)
                        Console.WriteLine($"line: {index}");
#endif
                    if (DateTime.TryParse(val.AttendanceDate, out DateTime actualAttendanceDateValue))
                    {
                        context.Add(new Attendance()
                        {
                            AttendanceCode = val.AttendanceCode,
                            AttendanceDate = actualAttendanceDateValue,
                            StudentId = val.StudentId
                        });
                        if (count++ == 5000)
                        {
                            context.SaveChanges();
                            //context.ChangeTracker.Clear();
                            count = 0;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error at line: {index} with wrong datetime format");
                    }
                });
            context.SaveChanges();
        }
        public static void ImportSequentially_ReduceMemoryUsage_ReleaseContext(this string inputFileFullPath, TestDBContext context)
        {
            int count = 0;
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            inputFileFullPath
                .EnumerateLines<AttendanceCSVDto>(cancellationTokenSource.Token)
                .ForAll((val, index) =>
                {
#if DEBUG
                    if (index % 50000 == 0)
                        Console.WriteLine($"line: {index}");
#endif
                    if (DateTime.TryParse(val.AttendanceDate, out DateTime actualAttendanceDateValue))
                    {
                        context.Add(new Attendance()
                        {
                            AttendanceCode = val.AttendanceCode,
                            AttendanceDate = actualAttendanceDateValue,
                            StudentId = val.StudentId
                        });
                        if (count++ == 5000)
                        {
                            context.SaveChanges();
                            context.ChangeTracker.Clear();
                            count = 0;
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Error at line: {index} with wrong datetime format");
                    }
                });
            context.SaveChanges();
        }
    }
}
