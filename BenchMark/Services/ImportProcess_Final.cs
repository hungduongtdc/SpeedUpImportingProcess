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
    public static class ImportProcess_Final
    {
        public static async Task<IEnumerable<ImportLogDto>> ImportAsync(this string inputFileFullPath, Func<TestDBContext> contextFactory)
        {
            using CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            ConcurrentBag<ImportLogDto> validateLogs = new();

            var importLogs = await inputFileFullPath
                     .EnumerateLines<AttendanceCSVDto>(cancellationTokenSource.Token)
                     .Select((val, index) =>
                     {

                         val.Index = index;
                         return val;
                     })
                     .Where(Validate(validateLogs))
                     .Partition(4000)
                     .TransformAsync(
                            transformer: ImportAction(contextFactory()),
                            maxDegreeOfParallelism: Environment.ProcessorCount,
                            transformToResult: x => x
                            );

            return validateLogs.Concat(importLogs);
        }

        private static Func<AttendanceCSVDto, bool> Validate(ConcurrentBag<ImportLogDto> importLog)
        {
            return x =>
            {
                if (!DateTime.TryParse(x.AttendanceDate, out DateTime _))
                {
                    importLog.Add(new ImportLogDto()
                    {
                        LineIndex = x.Index,
                        LogDetail = "Invalid datetime format"
                    });
                    return false;
                }
                return true;
            };
        }

        public static Func<IEnumerable<AttendanceCSVDto>, Task<IEnumerable<ImportLogDto>>> ImportAction(TestDBContext masterContext)
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
                return await masterContext.BulkCopyAsync<ImportLogDto>(new BulkCopyParametersDto()
                {
                    FinalizeProcedure = "dbo.ImportAttendance",
                    TempTables = new BulkCopyTempTableDto[] { tempTableDefinition },
                    Option = new BulkCopyOption()
                });

            };
        }
    }
}
