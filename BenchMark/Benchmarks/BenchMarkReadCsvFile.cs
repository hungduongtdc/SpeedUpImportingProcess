using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using SpeedUpImportingProcess.BenchMark.Contexts;
using SpeedUpImportingProcess.BenchMark.Contexts.Models;
using SpeedUpImportingProcess.BenchMark.DTO;
using SpeedUpImportingProcess.BenchMark.Extensions;
using SpeedUpImportingProcess.BenchMark.Extensions.CSV;
using SpeedUpImportingProcess.BenchMark.Extensions.DTO;
using SpeedUpImportingProcess.BenchMark.Extensions.Enumeration;
using SpeedUpImportingProcess.BenchMark.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace SpeedUpImportingProcess.BenchMark.Benchmarks
{
    [MemoryDiagnoser]
    public class BenchMarkReadCsvFile
    {
        private DbContextOptionsBuilder<TestDBContext> _dbContextOptionsBuilder;

        public string FileFullName { get; set; } = @"D:\3318343\Attendance_export_80k.txt";

        TestDBContext NewDbContext() => new TestDBContext(_dbContextOptionsBuilder.Options);

        public BenchMarkReadCsvFile GlobalSetup()
        {
            Setup();
            return this;
        }

        [GlobalSetup]
        public void Setup()
        {
            string SQLConnectionString = "Data Source=.\\SQLEXPRESS;Initial Catalog=TestDB;User Id=sa;Password=123; Persist Security Info=True;";

            this._dbContextOptionsBuilder = new DbContextOptionsBuilder<TestDBContext>()
                .UseSqlServer(SQLConnectionString, opt => opt.EnableRetryOnFailure(10));
            using (var context = NewDbContext())
            {
                context.Database.ExecuteSqlRaw("truncate table Attendance");
            }
        }

        [Benchmark(Baseline = true)]
        public void ImportSequentially_ReadAllFile()
        {
            using (var context = NewDbContext())
            {
                FileFullName.SequentiallyImport(context);
            }
        }
        [Benchmark]
        public void ImportSequentially_ReduceMemoryUsage()
        {
            using (var context = NewDbContext())
            {
                FileFullName.ImportSequentially_ReduceMemoryUsage(context);
            }
        }
        public void ImportSequentially_ReduceMemoryUsage_ReleaseContext()
        {
            using (var context = NewDbContext())
            {
                FileFullName.ImportSequentially_ReduceMemoryUsage_ReleaseContext(context);
            }
        }
        [Benchmark]
        public void ImportSequentially_ReduceMemoryUsage_BulkSave()
        {
            using (var context = NewDbContext())
            {
                FileFullName.ImportSequentially_ReduceMemoryUsage_BulkSave(context);
            }
        }

        [Benchmark]
        //[Benchmark(Baseline = true)]
        public void ImportParallel()
        {
            FileFullName.ImportParallel(NewDbContext);
        }

        [Benchmark]
        public async Task ImportParallelAsync()
        {
            await FileFullName.ImportParallelAsync(NewDbContext);
        }

        [Benchmark]
        public async Task ImportParallelAsyncBulkCopy()
        {
            await FileFullName.ImportParallelAsyncBulkCopyAsync(NewDbContext);
        }

        [Benchmark]
        public async Task FinalSolution()
        {
            await FileFullName.ImportAsync(NewDbContext);
        }
    }
}