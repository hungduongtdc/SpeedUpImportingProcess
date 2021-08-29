using BenchmarkDotNet.Running;
using SpeedUpImportingProcess.BenchMark.Benchmarks;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpeedUpImportingProcess.BenchMark
{
    static class Program
    {
        static void Main(string[] args)
        {
#if DEBUG
            //new BenchMarkReadCsvFile()
            //{
            //    FileFullName = @"D:\3318343\Attendance_export_200k.txt"
            //}
            //  .GlobalSetup().ImportSequentially_ReadAllFile();

            //new BenchMarkReadCsvFile()
            //{
            //    FileFullName = @"D:\3318343\Attendance_export_200k.txt"
            //}
            //.GlobalSetup().ImportSequentially_ReduceMemoryUsage();

            //  new BenchMarkReadCsvFile()
            //  {
            //      FileFullName = @"D:\3318343\Attendance_export_200k.txt"
            //  }
            //.GlobalSetup().ImportSequentially_ReduceMemoryUsage_ReleaseContext();


            //new BenchMarkReadCsvFile()
            //{
            //    FileFullName = @"D:\3318343\Attendance_export_200k.txt"
            //}
            //.GlobalSetup()
            //.ImportParallel();

            //new BenchMarkReadCsvFile()
            //{
            //    FileFullName = @"D:\3318343\Attendance_export_200k.txt"
            //}
            //.GlobalSetup()
            //.ImportParallelAsync()
            //.GetAwaiter()
            //.GetResult();

          //  new BenchMarkReadCsvFile()
          //  {
          //      FileFullName = @"D:\3318343\Attendance_export_200k.txt"
          //  }
          //.GlobalSetup()
          //.ImportParallelAsync_TableValued()
          //.GetAwaiter()
          //.GetResult();

            //  new BenchMarkReadCsvFile()
            //{
            //    FileFullName = @"D:\3318343\Attendance_export_200k.txt"
            //}
            //.GlobalSetup()
            //.ImportParallelAsyncBulkCopy()
            //.GetAwaiter()
            //.GetResult();

            //  new BenchMarkReadCsvFile()
            //  {
            //      FileFullName = @"d:\3318343\attendance_export_200k.txt"
            //  }
            //.GlobalSetup()
            //.FinalSolution()
            //.GetAwaiter()
            //.GetResult();

            Console.WriteLine("Finished");

#else
            var summary = BenchmarkRunner.Run<BenchMarkReadCsvFile>();
#endif
            Console.ReadLine();
        }

    }
}
