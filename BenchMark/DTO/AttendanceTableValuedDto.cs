using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeedUpImportingProcess.BenchMark.DTO
{
    public class AttendanceTableValuedDto
    {
        public int Index { get; set; }
        public int StudentId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int AttendanceCode { get; set; }
    }
}
