using CsvHelper.Configuration.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.DTO
{
    public class AttendanceCSVDto
    {
        [Ignore]
        public int Index { get; set; }
        [Index(3)]
        public string AttendanceDate { get; set; }
        [Index(6)]
        public int AttendanceCode { get; set; }
        [Index(16)]
        public int StudentId { get; set; }
      
    }
}
