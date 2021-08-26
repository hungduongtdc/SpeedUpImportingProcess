using System;
using System.Collections.Generic;

#nullable disable

namespace SpeedUpImportingProcess.BenchMark.Contexts.Models
{
    public partial class Attendance
    {
        public long Id { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int AttendanceCode { get; set; }
        public int StudentId { get; set; }
    }
}
