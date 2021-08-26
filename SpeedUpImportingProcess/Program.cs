using SpeedUpImportingProcess.Dto;
using SpeedUpImportingProcess.Extensions.CSVEx;
using System;
using System.Threading;

namespace SpeedUpImportingProcess
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileFullName = @"D:\3318343\Attendance_export.txt";

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

            int linecount = 0;
            var lineDatas = fileFullName.EnumerateLines<AttendanceCsvDto>(cancellationTokenSource.Token);

            var enumerator = lineDatas.GetEnumerator();

            while (true)
            {
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                Console.WriteLine($"press enter to read");
                Console.ReadLine();

                for (int i = 0; i < 500000; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }
                    linecount++;
                }

                Console.WriteLine($"{linecount}");
                if (!enumerator.MoveNext())
                {
                    break;
                }
            }


            Console.WriteLine($"{linecount}");
            Console.ReadLine();
        }
    }
}
