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
using Dapper;
using Microsoft.EntityFrameworkCore;
using SpeedUpImportingProcess.BenchMark.Extensions;

namespace SpeedUpImportingProcess.BenchMark.Services
{
    public static class ImportParallelAsync_TableValuedService
    {
        public static Task ImportParallelAsync_TableValued(this string inputFileFullPath, Func<DbContext> dbContextFactory)
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
                   .SafeParallelProcessAsync(ImportAction(dbContextFactory), Environment.ProcessorCount);
        }
        public static Func<AttendanceCSVDto[], Task> ImportAction(Func<DbContext> dbContextFactory)
        {

            return async partition =>
            {
                using (var context = dbContextFactory())
                {
                    await context.ExecuteAsync("ImportAttendance_TableValued", new
                    {
                        data = partition.Select(c => new AttendanceTableValuedDto()
                        {
                            Index = c.Index,
                            AttendanceCode = c.AttendanceCode,
                            AttendanceDate = DateTime.Parse(c.AttendanceDate),
                            StudentId = c.StudentId
                        }).ToDataTable()
                    });
                }
            };
        }
    }
}
