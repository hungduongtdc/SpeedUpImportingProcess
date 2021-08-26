using SpeedUpImportingProcess.BenchMark.Contexts;
using SpeedUpImportingProcess.BenchMark.Contexts.Models;
using SpeedUpImportingProcess.BenchMark.DTO;
using SpeedUpImportingProcess.BenchMark.Extensions;
using SpeedUpImportingProcess.BenchMark.Extensions.CSV;
using SpeedUpImportingProcess.BenchMark.Extensions.DTO;
using SpeedUpImportingProcess.BenchMark.Extensions.Enumeration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.Services
{
    public static class ParallelImportAsyncBulkCopyService
    {
        public static Task ImportParallelAsyncBulkCopyAsync(this string inputFileFullPath, Func<TestDBContext> contextFactory)
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
                   .SafeParallelProcessAsync(ImportAction(contextFactory()), Environment.ProcessorCount);
        }
        public static Func<AttendanceCSVDto[], Task> ImportAction(TestDBContext masterContext)
        {

            return async partition =>
            {
                string tempTableName = "#temp";
                string tempTableCreationScript = $"CREATE TABLE {tempTableName}([Index] int, AttendanceDate date,AttendanceCode int,StudentId int)";
                var tempTableDefinition = new BulkCopyTempTableDto()
                {
                    CreationScript = tempTableCreationScript,
                    Data = partition,
                    TempTableName = tempTableName
                };
                await masterContext.BulkCopyAsync<int>(new BulkCopyParametersDto()
                {
                    FinalizeProcedure = "dbo.ImportAttendance",
                    TempTables = new BulkCopyTempTableDto[] { tempTableDefinition },
                    Option = new BulkCopyOption()
                });

            };
        }
    }
}
